using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Dtos.SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Services
{
    public class NoteService : INoteService
    {
        private readonly ApplicationDbContext _db;

        public NoteService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// 获取当前用户可访问的所有笔记（包含协作工作区）
        /// </summary>
        public async Task<IEnumerable<NoteViewDto>> GetUserNotesAsync(int userId)
        {
            // 找出用户可访问的所有工作区（自己 + 成员身份）
            var accessibleWorkspaceIds = await _db.WorkspaceMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.WorkspaceId)
                .Union(
                    _db.Workspaces.Where(w => w.OwnerUserId == userId).Select(w => w.Id)
                )
                .Distinct()
                .ToListAsync();

            // 获取这些工作区下的笔记
            var notes = await _db.Notes
                .Where(n => accessibleWorkspaceIds.Contains(n.WorkspaceId) && !n.IsDeleted)
                .OrderByDescending(n => n.LastUpdateTime)
                .Select(n => new NoteViewDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    ContentMd = n.ContentMd,
                    ContentHtml = n.ContentHtml,
                    CanvasDataJson = n.CanvasDataJson,
                    CreateTime = n.CreateTime,
                    LastUpdateTime = n.LastUpdateTime,
                    WorkspaceId = n.WorkspaceId,
                    IsDeleted = n.IsDeleted,
                    DeletedTime = n.DeletedTime
                })
                .ToListAsync();

            return notes;
        }

        /// <summary>
        /// 创建笔记（用户必须是该工作区成员或创建者）
        /// </summary>
        public async Task<int> CreateNoteAsync(int userId, NoteCreateDto dto)
        {
            var hasAccess = await _db.Workspaces.AnyAsync(w => w.Id == dto.WorkspaceId && w.OwnerUserId == userId)
                || await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == dto.WorkspaceId && m.UserId == userId && m.CanEdit);

            if (!hasAccess)
                throw new UnauthorizedAccessException("无权在该工作区创建笔记。");

            var note = new Note
            {
                WorkspaceId = dto.WorkspaceId,
                Title = dto.Title,
                ContentMd = dto.ContentMd,
                ContentHtml = dto.ContentHtml,
                CanvasDataJson = dto.CanvasDataJson,
                Type = dto.Type,
                CreateTime = DateTime.UtcNow,
                LastUpdateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            _db.Notes.Add(note);
            await _db.SaveChangesAsync();

            return note.Id;
        }

        /// <summary>
        /// 更新笔记（仅 Owner 或具有编辑权限的成员可修改）
        /// </summary>
        public async Task<int> UpdateNoteAsync(int noteId, int userId, NoteUpdateDto dto)
        {
            var note = await _db.Notes
                .Include(n => n.Workspace)
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
                throw new KeyNotFoundException("未找到笔记。");

            bool canEdit = note.Workspace.OwnerUserId == userId ||
                           await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == note.WorkspaceId && m.UserId == userId && m.CanEdit);

            if (!canEdit)
                throw new UnauthorizedAccessException("无权编辑该笔记。");

            if (!string.IsNullOrWhiteSpace(dto.Title))
                note.Title = dto.Title;

            note.ContentMd = dto.ContentMd ?? note.ContentMd;
            note.ContentHtml = dto.ContentHtml ?? note.ContentHtml;
            note.CanvasDataJson = dto.CanvasDataJson ?? note.CanvasDataJson;
            note.LastUpdateTime = DateTime.UtcNow;

            return await _db.SaveChangesAsync();
        }

        /// <summary>
        /// 软删除笔记（仅 Owner 或具有编辑权限的成员）
        /// </summary>
        public async Task<int> SoftDeleteAsync(IEnumerable<int> noteIds, int userId)
        {
            var notes = await _db.Notes
                .Include(n => n.Workspace)
                .Where(n => noteIds.Contains(n.Id))
                .ToListAsync();

            var deletableNotes = new List<Note>();

            foreach (var note in notes)
            {
                bool canEdit = note.Workspace.OwnerUserId == userId ||
                               await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == note.WorkspaceId && m.UserId == userId && m.CanEdit);

                if (canEdit && !note.IsDeleted)
                {
                    note.IsDeleted = true;
                    note.DeletedTime = DateTime.UtcNow;
                    note.LastUpdateTime = DateTime.UtcNow;
                    deletableNotes.Add(note);
                }
            }

            if (!deletableNotes.Any())
                return 0;

            return await _db.SaveChangesAsync();
        }
    }
}
