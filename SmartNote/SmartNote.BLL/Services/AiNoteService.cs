using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.BLL.Ai;
using SmartNote.DAL;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Services
{
    public class AiNoteService : IAiNoteService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly ApplicationDbContext _db;
        private readonly OpenAiClient _ai;

        public AiNoteService(ApplicationDbContext db, OpenAiClient ai)
        {
            _db = db;
            _ai = ai;
        }

        public async Task<AiSummaryResponse> GenerateSummaryAsync(int userId, AiSummaryRequest request)
        {
            if (request.NoteId <= 0) throw new BusinessException("NoteId 无效。");

            var note = await GetNoteForUserAsync(userId, request.NoteId);
            var text = ExtractPrimaryText(note.Type, note.ContentJson);
            text = Truncate(text, 6000);

            var maxLen = request.MaxLength <= 0 ? 100 : Math.Min(request.MaxLength, 200);

            var system = "你是一个智能摘要助手。必须严格输出 JSON，不要输出任何解释、代码块或多余字符。";
            var user = $$"""
请根据下面的笔记生成中文摘要，摘要不超过 {{maxLen}} 字，尽量覆盖核心信息，避免空泛。
返回格式必须是：
{"summary":"..."}

【标题】
{{note.Title}}

【内容】
{{text}}
""";

            var json = await _ai.ChatJsonAsync(system, user);

            try
            {
                using var doc = JsonDocument.Parse(json);
                var summary = doc.RootElement.GetProperty("summary").GetString() ?? string.Empty;
                summary = summary.Trim().Replace("\n", " ").Replace("\r", " ");
                if (summary.Length > maxLen) summary = summary[..maxLen];

                return new AiSummaryResponse { Summary = summary };
            }
            catch (Exception)
            {
                throw new BusinessException("AI 返回摘要格式不正确。");
            }
        }

        public async Task<AiKnowledgeExtensionResponse> GenerateKnowledgeExtensionAsync(int userId, AiKnowledgeExtensionRequest request)
        {
            if (request.NoteId <= 0) throw new BusinessException("NoteId 无效。");

            var note = await GetNoteForUserAsync(userId, request.NoteId);
            var tags = note.NoteTags.Select(nt => nt.Tag.Name).Distinct().ToList();

            var text = ExtractPrimaryText(note.Type, note.ContentJson);
            text = Truncate(text, 6000);

            var maxItems = request.MaxItems <= 0 ? 5 : Math.Clamp(request.MaxItems, 3, 8);

            var system = "你是一个学习路线规划助手。必须严格输出 JSON，不要输出任何解释、代码块或多余字符。";
            var user = $$"""
请根据笔记内容和标签，推荐 {{Math.Min(5, maxItems)}}~{{maxItems}} 个“进阶知识点”，每个知识点要具体、可行动。
返回格式必须是：
{
  "items":[
    {"title":"知识点标题","description":"为什么推荐 + 学什么/怎么学（1-2 句）"}
  ]
}

【标签】
{{(tags.Count == 0 ? "（无）" : string.Join(", ", tags))}}

【标题】
{{note.Title}}

【内容】
{{text}}
""";

            var json = await _ai.ChatJsonAsync(system, user);

            try
            {
                using var doc = JsonDocument.Parse(json);
                var itemsEl = doc.RootElement.GetProperty("items");
                if (itemsEl.ValueKind != JsonValueKind.Array)
                    throw new Exception("items not array");

                var items = JsonSerializer.Deserialize<List<AiKnowledgePointDto>>(itemsEl.GetRawText(), JsonOptions) ?? new List<AiKnowledgePointDto>();

                // 兜底：过滤空项并限制数量
                items = items
                    .Where(i => !string.IsNullOrWhiteSpace(i.Title))
                    .Select(i => new AiKnowledgePointDto
                    {
                        Title = i.Title.Trim(),
                        Description = (i.Description ?? string.Empty).Trim()
                    })
                    .Take(maxItems)
                    .ToList();

                if (items.Count == 0)
                    throw new Exception("empty items");

                return new AiKnowledgeExtensionResponse { Items = items };
            }
            catch (Exception)
            {
                throw new BusinessException("AI 返回知识扩展格式不正确。");
            }
        }

        public async Task<MindMapGraphDto> GenerateMindMapAsync(int userId, AiTextToMindMapRequest request)
        {
            if (request.NoteId <= 0) throw new BusinessException("NoteId 无效。");

            var note = await GetNoteForUserAsync(userId, request.NoteId);
            if (note.Type != NoteType.Markdown)
                throw new BusinessException("仅支持从 Markdown 笔记生成思维导图。");

            var markdown = ExtractMarkdown(note.ContentJson);
            markdown = Truncate(markdown, 12000);

            var maxNodes = request.MaxNodes <= 0 ? 80 : Math.Clamp(request.MaxNodes, 20, 200);

            var system = "你是一个将 Markdown 转为思维导图大纲的助手。必须严格输出 JSON，不要输出任何解释、代码块或多余字符。";
            var user = $$"""
请把下面的 Markdown 笔记转换为思维导图“大纲树”JSON。
要求：
1) 只允许输出一个 JSON 对象；
2) JSON 结构必须为：
{
  "root":"根节点文本",
  "children":[
    {"text":"节点文本","children":[...]}
  ]
}
3) 节点文本尽量精炼（<= 20 字），总节点数不要超过 {{maxNodes}}；
4) 优先根据 Markdown 的 H1/H2/H3 与列表层级组织；段落可归纳成 1-2 个要点放到最近标题下。

【Markdown】
{{markdown}}
""";

            var json = await _ai.ChatJsonAsync(system, user);

            MindMapOutline outline;
            try
            {
                outline = JsonSerializer.Deserialize<MindMapOutline>(json, JsonOptions)
                          ?? throw new Exception("outline null");
            }
            catch (Exception)
            {
                throw new BusinessException("AI 返回的思维导图大纲格式不正确。");
            }

            if (string.IsNullOrWhiteSpace(outline.Root))
                outline.Root = string.IsNullOrWhiteSpace(note.Title) ? "思维导图" : note.Title;

            outline.Children ??= new List<MindMapOutlineNode>();

            // 后端生成 nodes/edges（与 NoteType.MindMap ContentJson 对齐）
            return BuildGraph(outline, maxNodes);
        }

        public async Task<AiQuizResponse> GenerateQuizAsync(int userId, AiQuizRequest request)
        {
            if (request.NoteId <= 0) throw new BusinessException("NoteId 无效。");

            var note = await GetNoteForUserAsync(userId, request.NoteId);
            var text = ExtractPrimaryText(note.Type, note.ContentJson);
            text = Truncate(text, 8000);

            var count = request.Count <= 0 ? 3 : Math.Clamp(request.Count, 1, 10);

            var system = "你是一个智能出题器。必须严格输出 JSON，不要输出任何解释、代码块或多余字符。";
            var user = $$"""
请基于下面的笔记内容生成 {{count}} 道单选题（每题 4 个选项），并给出正确答案索引和解析。
返回格式必须是：
{
  "questions":[
    {
      "question":"题干",
      "options":["选项A","选项B","选项C","选项D"],
      "answerIndex":0,
      "explanation":"解析（1-2 句）"
    }
  ]
}
要求：
1) answerIndex 必须是 0~3；
2) options 必须刚好 4 个；
3) 题目覆盖核心概念/易错点，避免太简单。

【标题】
{{note.Title}}

【内容】
{{text}}
""";

            var json = await _ai.ChatJsonAsync(system, user);

            try
            {
                using var doc = JsonDocument.Parse(json);
                var qEl = doc.RootElement.GetProperty("questions");
                if (qEl.ValueKind != JsonValueKind.Array)
                    throw new Exception("questions not array");

                var questions = JsonSerializer.Deserialize<List<AiQuizQuestionDto>>(qEl.GetRawText(), JsonOptions) ?? new List<AiQuizQuestionDto>();

                questions = questions
                    .Where(q => !string.IsNullOrWhiteSpace(q.Question))
                    .Select(q => new AiQuizQuestionDto
                    {
                        Question = q.Question.Trim(),
                        Options = (q.Options ?? new List<string>()).Select(o => o.Trim()).Where(o => o.Length > 0).Take(4).ToList(),
                        AnswerIndex = q.AnswerIndex,
                        Explanation = (q.Explanation ?? string.Empty).Trim()
                    })
                    .Where(q => q.Options.Count == 4 && q.AnswerIndex >= 0 && q.AnswerIndex <= 3)
                    .Take(count)
                    .ToList();

                if (questions.Count == 0)
                    throw new Exception("empty questions");

                return new AiQuizResponse { Questions = questions };
            }
            catch (Exception)
            {
                throw new BusinessException("AI 返回的题目格式不正确。");
            }
        }

        // -------------------------
        // 数据与权限
        // -------------------------

        private async Task<Domain.Entities.Note> GetNoteForUserAsync(int userId, int noteId)
        {
            var note = await _db.Notes
                .AsNoTracking()
                .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
                .Include(n => n.Workspace).ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(n => n.Id == noteId &&
                                          (n.Workspace.OwnerUserId == userId ||
                                           n.Workspace.Members.Any(m => m.UserId == userId)));

            if (note == null)
                throw new KeyNotFoundException("未找到笔记或无权限。");

            return note;
        }

        // -------------------------
        // 内容抽取
        // -------------------------

        private static string ExtractMarkdown(string contentJson)
        {
            var raw = ExtractFromJsonString(contentJson, "md");
            return string.IsNullOrWhiteSpace(raw) ? contentJson : raw;
        }

        private static string ExtractPrimaryText(NoteType type, string contentJson)
        {
            if (string.IsNullOrWhiteSpace(contentJson))
                return string.Empty;

            // 优先按 NoteType 取字段
            if (type == NoteType.Markdown)
            {
                var md = ExtractFromJsonString(contentJson, "md");
                if (!string.IsNullOrWhiteSpace(md)) return md;
                var html = ExtractFromJsonString(contentJson, "html");
                if (!string.IsNullOrWhiteSpace(html)) return html;
            }

            if (type == NoteType.RichText)
            {
                var content = ExtractFromJsonString(contentJson, "content");
                if (!string.IsNullOrWhiteSpace(content)) return content;
            }

            // 兜底
            return contentJson;
        }

        private static string ExtractFromJsonString(string json, string propertyName)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return string.Empty;

                if (!doc.RootElement.TryGetProperty(propertyName, out var value))
                    return string.Empty;

                return value.ValueKind == JsonValueKind.String ? (value.GetString() ?? string.Empty) : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string Truncate(string text, int maxChars)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Length <= maxChars) return text;
            return text[..maxChars];
        }

        // -------------------------
        // MindMap Graph 生成
        // -------------------------

        private static MindMapGraphDto BuildGraph(MindMapOutline outline, int maxNodes)
        {
            var graph = new MindMapGraphDto();

            // root
            var rootId = "root";
            var root = new GraphNode
            {
                Id = rootId,
                Text = outline.Root.Trim(),
                Children = new List<GraphNode>()
            };

            // 先为 Outline 分配稳定的唯一 Id（n1/n2/...），再统一做布局
            var idCounter = 0;
            var remaining = Math.Max(1, maxNodes) - 1; // root 已占 1 个
            foreach (var child in outline.Children ?? new List<MindMapOutlineNode>())
            {
                if (remaining <= 0) break;
                var node = BuildGraphNode(child, ref idCounter, ref remaining);
                if (node != null) root.Children.Add(node);
            }

            var currentY = 0.0;
            LayoutAndAppend(root, parentId: null, depth: 0, ref currentY, graph);

            return graph;
        }

        private static GraphNode? BuildGraphNode(MindMapOutlineNode outlineNode, ref int idCounter, ref int remaining)
        {
            if (remaining <= 0) return null;

            idCounter++;
            remaining--;

            var node = new GraphNode
            {
                Id = $"n{idCounter}",
                Text = (outlineNode.Text ?? string.Empty).Trim(),
                Children = new List<GraphNode>()
            };

            foreach (var child in outlineNode.Children ?? new List<MindMapOutlineNode>())
            {
                if (remaining <= 0) break;
                var childNode = BuildGraphNode(child, ref idCounter, ref remaining);
                if (childNode != null) node.Children.Add(childNode);
            }

            return node;
        }

        private static double LayoutAndAppend(GraphNode node, string? parentId, int depth, ref double currentY, MindMapGraphDto graph)
        {
            var childYs = new List<double>();
            foreach (var child in node.Children)
            {
                childYs.Add(LayoutAndAppend(child, node.Id, depth + 1, ref currentY, graph));
            }

            var y = childYs.Count == 0
                ? currentY
                : (childYs.Min() + childYs.Max()) / 2.0;

            if (childYs.Count == 0)
                currentY += 120;

            var x = depth * 260.0;

            graph.Nodes.Add(new MindMapNodeDto
            {
                Id = node.Id,
                Position = new MindMapPositionDto { X = x, Y = y },
                Data = new MindMapNodeDataDto { Label = node.Text },
                Type = null
            });

            if (!string.IsNullOrWhiteSpace(parentId))
            {
                graph.Edges.Add(new MindMapEdgeDto
                {
                    Id = $"e-{parentId}-{node.Id}",
                    Source = parentId,
                    Target = node.Id,
                    Type = null
                });
            }

            return y;
        }

        private sealed class GraphNode
        {
            public string Id { get; init; } = string.Empty;
            public string Text { get; init; } = string.Empty;
            public List<GraphNode> Children { get; init; } = new();
        }

        private sealed class MindMapOutline
        {
            public string Root { get; set; } = string.Empty;
            public List<MindMapOutlineNode>? Children { get; set; }
        }

        private sealed class MindMapOutlineNode
        {
            public string Text { get; set; } = string.Empty;
            public List<MindMapOutlineNode>? Children { get; set; }
        }
    }
}
