using System.Text.Json.Serialization;

namespace SmartNote.Shared.Dtos
{
    public class AiSummaryRequest
    {
        public int NoteId { get; set; }

        /// <summary>
        /// 摘要最大长度（字符/字数），默认 100
        /// </summary>
        public int MaxLength { get; set; } = 100;
    }

    public class AiSummaryResponse
    {
        public string Summary { get; set; } = string.Empty;
    }

    public class AiKnowledgeExtensionRequest
    {
        public int NoteId { get; set; }

        /// <summary>
        /// 推荐条数（建议 3-5）
        /// </summary>
        public int MaxItems { get; set; } = 5;
    }

    public class AiKnowledgePointDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AiKnowledgeExtensionResponse
    {
        public List<AiKnowledgePointDto> Items { get; set; } = new();
    }

    public class AiTextToMindMapRequest
    {
        public int NoteId { get; set; }

        /// <summary>
        /// 最大节点数（防止过大），默认 80
        /// </summary>
        public int MaxNodes { get; set; } = 80;
    }

    /// <summary>
    /// 供 MindMapEditor/ReactFlow 使用的图结构（与 NoteType.MindMap 的 ContentJson 保持一致：nodes/edges）
    /// </summary>
    public class MindMapGraphDto
    {
        public List<MindMapNodeDto> Nodes { get; set; } = new();
        public List<MindMapEdgeDto> Edges { get; set; } = new();
    }

    public class MindMapNodeDto
    {
        public string Id { get; set; } = string.Empty;
        public MindMapPositionDto Position { get; set; } = new();
        public MindMapNodeDataDto Data { get; set; } = new();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }
    }

    public class MindMapPositionDto
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class MindMapNodeDataDto
    {
        public string Label { get; set; } = string.Empty;
    }

    public class MindMapEdgeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }
    }

    public class AiQuizRequest
    {
        public int NoteId { get; set; }

        /// <summary>
        /// 题目数量（默认 3）
        /// </summary>
        public int Count { get; set; } = 3;
    }

    public class AiQuizQuestionDto
    {
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();

        /// <summary>
        /// 正确答案索引（0-3）
        /// </summary>
        public int AnswerIndex { get; set; }

        public string Explanation { get; set; } = string.Empty;
    }

    public class AiQuizResponse
    {
        public List<AiQuizQuestionDto> Questions { get; set; } = new();
    }
}
