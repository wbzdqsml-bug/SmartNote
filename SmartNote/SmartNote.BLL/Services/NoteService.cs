using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Shared.Dtos;
using SmartNote.Domain.Exceptions;

namespace SmartNote.BLL.Services
{
    public class NoteService : INoteService
    {
        private readonly ApplicationDbContext _db;

        public NoteService(ApplicationDbContext db)
        {
            _db = db;
        }

        private string GetDefaultContentJson(NoteType type)
        {
            return type switch
            {
                NoteType.Markdown => "{\"md\": \"\", \"html\": \"\"}",
                NoteType.Canvas => "{\"elements\": []}",
                NoteType.MindMap => "{\"nodes\": [], \"edges\": []}",
                NoteType.RichText => "{\"content\": \"\"}",
                _ => "{}"
            };
        }

        public async Task<IEnumerable<NoteViewDto>> GetUserNotesAsync(int userId)
        {
            var accessibleWorkspaceIds = await _db.WorkspaceMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.WorkspaceId)
                .Union(
                    _db.Workspaces.Where(w => w.OwnerUserId == userId).Select(w => w.Id)
                )
                .Distinct()
                .ToListAsync();

            var notes = await _db.Notes
                .Where(n => accessibleWorkspaceIds.Contains(n.WorkspaceId) && !n.IsDeleted)
                .OrderByDescending(n => n.LastUpdateTime)
                .Select(n => new NoteViewDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Type = n.Type,
                    ContentJson = n.ContentJson,
                    CreateTime = n.CreateTime,
                    LastUpdateTime = n.LastUpdateTime,
                    WorkspaceId = n.WorkspaceId,
                    IsDeleted = n.IsDeleted,
                    DeletedTime = n.DeletedTime
                })
                .ToListAsync();

            return notes;
        }

        public async Task<int> CreateNoteAsync(int userId, NoteCreateDto dto)
        {
            bool hasAccess = await _db.Workspaces.AnyAsync(w => w.Id == dto.WorkspaceId && w.OwnerUserId == userId)
                || await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == dto.WorkspaceId && m.UserId == userId && m.CanEdit);

            if (!hasAccess)
                throw new PermissionDeniedException("无权在该工作区创建笔记。");

            var note = new Note
            {
                WorkspaceId = dto.WorkspaceId,
                Title = dto.Title,
                Type = dto.Type,
                ContentJson = GetDefaultContentJson(dto.Type),
                CreateTime = DateTime.UtcNow,
                LastUpdateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            _db.Notes.Add(note);
            await _db.SaveChangesAsync();

            return note.Id;
        }

        public async Task<int> UpdateNoteAsync(int noteId, int userId, NoteUpdateDto dto)
        {
            var note = await _db.Notes.Include(n => n.Workspace).FirstOrDefaultAsync(n => n.Id == noteId);
            if (note == null)
                throw new KeyNotFoundException("未找到笔记。");

            bool canEdit = note.Workspace.OwnerUserId == userId ||
                           await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == note.WorkspaceId && m.UserId == userId && m.CanEdit);

            if (!canEdit)
                throw new PermissionDeniedException("无权编辑该笔记。");

            if (!string.IsNullOrWhiteSpace(dto.Title))
                note.Title = dto.Title;

            if (!string.IsNullOrWhiteSpace(dto.ContentJson))
                note.ContentJson = dto.ContentJson;

            note.LastUpdateTime = DateTime.UtcNow;

            return await _db.SaveChangesAsync();
        }

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
