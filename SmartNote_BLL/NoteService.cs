// SmartNote_BLL/NoteService.cs
using Markdig;
using Microsoft.EntityFrameworkCore;
using SmartNote_DAL;                   // (确保 using SmartNote_DAL)
using SmartNote_Domain;                 // (确保 using SmartNote_Domain)
using SmartNote_Shared.DTOs.Note;       // (确保 using SmartNote_Shared)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartNote_BLL // (确保命名空间正确)
{
    // 1. (关键) 确保类实现了 INoteService 接口
    public class NoteService : INoteService
    {
        private readonly ApplicationDbContext _context;
        // (我们将在迭代 5 添加 ILogger)

        // 2. 注入 DbContext
        public NoteService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 辅助方法 (用于权限检查) ---

        /// <summary>
        /// (私有辅助方法) 检查用户是否有权访问指定工作区
        /// </summary>
        private async Task<WorkspaceMember?> GetUserPermissionAsync(int workspaceId, int userId, string? requiredRole = null)
        {
            var member = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);

            if (member == null)
            {
                return null; // 用户不在此工作区
            }

            // (迭代 3 会在这里添加更复杂的 Role 检查)
            if (requiredRole != null)
            {
                if (requiredRole == "Member" && (member.Role == "Member" || member.Role == "Admin" || member.Role == "Owner"))
                {
                    return member;
                }
                // ... (其他角色检查) ...
                return null; // 权限不足
            }

            return member; // 只要是成员即可
        }

        // (辅助方法) 获取用户的个人工作区 (迭代 1 使用)
        private async Task<Workspace?> GetPersonalWorkspaceAsync(int userId)
        {
            return await _context.Workspaces
                .FirstOrDefaultAsync(w => w.OwnerUserId == userId && w.Type == "Personal");
        }

        // (辅助方法) Markdown 转 HTML
        private string ConvertMarkdownToHtml(string? markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }
            var pipeline = new Markdig.MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var html = Markdig.Markdown.ToHtml(markdown, pipeline);

            // (迭代 5 将在这里添加 HtmlSanitizer 清理)
            return html;
        }

        // --- 接口实现 ---

        /// <summary>
        /// 获取当前登录用户的所有笔记列表（预览）
        /// </summary>
        public async Task<List<NotePreviewDto>> GetNotesForUserAsync(int userId)
        {
            // 迭代 1：只获取个人工作区笔记
            var personalWorkspace = await GetPersonalWorkspaceAsync(userId);
            if (personalWorkspace == null)
            {
                // 如果连个人工作区都没有（理论上注册时应已创建），返回空列表
                return new List<NotePreviewDto>();
            }

            return await _context.Notes
                .Where(n => n.WorkspaceId == personalWorkspace.Id) // 只查询个人工作区
                .OrderByDescending(n => n.LastUpdateTime)
                .Select(n => new NotePreviewDto // 投影到 DTO
                {
                    Id = n.Id,
                    WorkspaceId = n.WorkspaceId,
                    Title = n.Title,
                    // (迭代 5 再实现 AI 摘要)
                    Summary = n.ContentMd != null ? (n.ContentMd.Length > 100 ? n.ContentMd.Substring(0, 100) + "..." : n.ContentMd) : "（空笔记）",
                    LastUpdateTime = n.LastUpdateTime
                })
                .ToListAsync();
        }

        /// <summary>
        /// 获取单篇笔记的详细内容（包含权限检查）
        /// </summary>
        public async Task<NoteDto> GetNoteByIdAsync(int noteId, int userId)
        {
            var note = await _context.Notes.FindAsync(noteId);
            if (note == null)
            {
                throw new KeyNotFoundException("笔记未找到。");
            }

            // (关键) 权限检查
            var permission = await GetUserPermissionAsync(note.WorkspaceId, userId);
            if (permission == null)
            {
                throw new UnauthorizedAccessException("你无权访问此笔记。");
            }

            // 映射到 DTO
            return new NoteDto
            {
                Id = note.Id,
                WorkspaceId = note.WorkspaceId,
                Title = note.Title,
                ContentMd = note.ContentMd,
                ContentHtml = note.ContentHtml,
                CreateTime = note.CreateTime,
                LastUpdateTime = note.LastUpdateTime
            };
        }

        /// <summary>
        /// 在指定工作区创建一篇新笔记（包含权限检查）
        /// </summary>
        public async Task<NoteDto> CreateNoteAsync(NoteCreateDto createDto, int userId)
        {
            // (关键) 权限检查：用户必须是该工作区的成员
            var permission = await GetUserPermissionAsync(createDto.WorkspaceId, userId);
            if (permission == null)
            {
                throw new UnauthorizedAccessException("你无权在此工作区创建笔记。");
            }

            var note = new Note
            {
                WorkspaceId = createDto.WorkspaceId,
                Title = createDto.Title,
                ContentMd = createDto.ContentMd,
                ContentHtml = ConvertMarkdownToHtml(createDto.ContentMd), // 转换 Markdown
                CreateTime = DateTime.UtcNow,
                LastUpdateTime = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // 返回新创建的笔记 DTO (从实体映射)
            return (await GetNoteByIdAsync(note.Id, userId)); // (复用 GetNoteByIdAsync 来映射)
        }

        /// <summary>
        /// 更新一篇笔记（包含权限检查）
        /// </summary>
        public async Task<NoteDto> UpdateNoteAsync(NoteUpdateDto updateDto, int userId)
        {
            var note = await _context.Notes.FindAsync(updateDto.Id);
            if (note == null)
            {
                throw new KeyNotFoundException("笔记未找到。");
            }

            // (关键) 权限检查
            var permission = await GetUserPermissionAsync(note.WorkspaceId, userId);
            if (permission == null)
            {
                throw new UnauthorizedAccessException("你无权修改此笔记。");
            }

            note.Title = updateDto.Title;
            note.ContentMd = updateDto.ContentMd;
            note.ContentHtml = ConvertMarkdownToHtml(updateDto.ContentMd); // 重新转换 Markdown
            note.LastUpdateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (await GetNoteByIdAsync(note.Id, userId)); // (复用 GetNoteByIdAsync 来映射)
        }

        /// <summary>
        /// 删除一篇笔记（包含权限检查）
        /// </summary>
        public async Task DeleteNoteAsync(int noteId, int userId) // 3. (关键) 返回类型为 Task
        {
            var note = await _context.Notes.FindAsync(noteId);
            if (note == null)
            {
                throw new KeyNotFoundException("笔记未找到。");
            }

            // (关键) 权限检查
            var permission = await GetUserPermissionAsync(note.WorkspaceId, userId);
            if (permission == null)
            {
                throw new UnauthorizedAccessException("你无权删除此笔记。");
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            // 4. (关键) 异步 Task 方法不需要显式 return
        }
    }
}