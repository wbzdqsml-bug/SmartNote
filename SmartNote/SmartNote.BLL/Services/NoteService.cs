using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;

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

        // =========================================
        // ★ ToDto：所有笔记统一转为 NoteViewDto
        // =========================================
        private static NoteViewDto ToDto(Note n)
        {
            return new NoteViewDto
            {
                Id = n.Id,
                Title = n.Title,
                Type = n.Type,
                ContentJson = n.ContentJson,
                WorkspaceId = n.WorkspaceId,
                CategoryId = n.CategoryId,
                CategoryName = n.Category?.Name,
                CategoryColor = n.Category?.Color,
                CreateTime = n.CreateTime,
                LastUpdateTime = n.LastUpdateTime,
                IsDeleted = n.IsDeleted,
                DeletedTime = n.DeletedTime,
                Tags = n.NoteTags
                    .Select(nt => new TagDto
                    {
                        Id = nt.Tag.Id,
                        Name = nt.Tag.Name,
                        Color = nt.Tag.Color
                    })
                    .ToList()
            };
        }

        // =========================================
        // 获取用户可访问的所有笔记
        // =========================================
        public async Task<IEnumerable<NoteViewDto>> GetUserNotesAsync(int userId)
        {
            var workspaceIds = await _db.WorkspaceMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.WorkspaceId)
                .Union(
                    _db.Workspaces.Where(w => w.OwnerUserId == userId).Select(w => w.Id)
                )
                .Distinct()
                .ToListAsync();

            var notes = await _db.Notes
                .Include(n => n.Category)
                .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
                .Where(n => workspaceIds.Contains(n.WorkspaceId) && !n.IsDeleted)
                .OrderByDescending(n => n.LastUpdateTime)
                .ToListAsync();

            return notes.Select(ToDto);
        }

        // =========================================
        // 获取单条笔记（完整信息）
        // =========================================
        public async Task<NoteViewDto?> GetNoteByIdAsync(int userId, int noteId)
        {
            var note = await _db.Notes
                .Include(n => n.Category)
                .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
                .Include(n => n.Workspace)
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
                return null;

            bool canView =
                   note.Workspace.OwnerUserId == userId ||
                   await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == note.WorkspaceId && m.UserId == userId);

            if (!canView)
                return null;

            return ToDto(note);
        }

        // =========================================
        // 分类 + 标签筛选
        // =========================================
        public async Task<IEnumerable<NoteViewDto>> FilterNotesAsync(
            int userId,
            int? categoryId,
            IReadOnlyList<int>? tagIds)
        {
            var workspaceIds = await _db.WorkspaceMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.WorkspaceId)
                .Union(_db.Workspaces.Where(w => w.OwnerUserId == userId).Select(w => w.Id))
                .Distinct()
                .ToListAsync();

            var query = _db.Notes
                .Include(n => n.Category)
                .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
                .Where(n => workspaceIds.Contains(n.WorkspaceId) && !n.IsDeleted)
                .AsQueryable();

            if (categoryId != null)
                query = query.Where(n => n.CategoryId == categoryId);

            if (tagIds != null && tagIds.Count > 0)
            {
                foreach (var tagId in tagIds)
                    query = query.Where(n => n.NoteTags.Any(nt => nt.TagId == tagId));
            }

            var list = await query.OrderByDescending(n => n.LastUpdateTime).ToListAsync();
            return list.Select(ToDto);
        }

        // =========================================
        // 创建笔记
        // =========================================
        public async Task<int> CreateNoteAsync(int userId, NoteCreateDto dto)
        {
            bool canCreate =
                   await _db.Workspaces.AnyAsync(w => w.Id == dto.WorkspaceId && w.OwnerUserId == userId) ||
                   await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == dto.WorkspaceId && m.UserId == userId && m.CanEdit);

            if (!canCreate)
                throw new UnauthorizedAccessException("无权在该工作区创建笔记。");

            var note = new Note
            {
                WorkspaceId = dto.WorkspaceId,
                Title = dto.Title,
                Type = dto.Type,
                ContentJson = GetDefaultContentJson(dto.Type),
                CategoryId = dto.CategoryId,
                CreateTime = DateTime.UtcNow,
                LastUpdateTime = DateTime.UtcNow
            };

            _db.Notes.Add(note);
            await _db.SaveChangesAsync();

            if (dto.TagIds != null && dto.TagIds.Any())
            {
                foreach (var tagId in dto.TagIds)
                {
                    _db.NoteTags.Add(new NoteTag
                    {
                        NoteId = note.Id,
                        TagId = tagId
                    });
                }
                await _db.SaveChangesAsync();
            }

            return note.Id;
        }

        // =========================================
        // 更新笔记（标题/内容/分类）
        // =========================================
        public async Task<int> UpdateNoteAsync(int noteId, int userId, NoteUpdateDto dto)
        {
            var note = await _db.Notes
                .Include(n => n.Workspace)
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
                throw new KeyNotFoundException("未找到笔记。");

            bool canEdit = note.Workspace.OwnerUserId == userId ||
                           await _db.WorkspaceMembers.AnyAsync(m =>
                                m.WorkspaceId == note.WorkspaceId &&
                                m.UserId == userId &&
                                m.CanEdit);

            if (!canEdit)
                throw new UnauthorizedAccessException("无权编辑该笔记。");

            if (!string.IsNullOrWhiteSpace(dto.Title))
                note.Title = dto.Title;

            if (!string.IsNullOrWhiteSpace(dto.ContentJson))
                note.ContentJson = dto.ContentJson;

            if (dto.CategoryId.HasValue)
                note.CategoryId = dto.CategoryId;

            note.LastUpdateTime = DateTime.UtcNow;

            return await _db.SaveChangesAsync();
        }


        // =========================================
        // 更新笔记标签（多对多）
        // =========================================
        public async Task UpdateNoteTagsAsync(int userId, int noteId, List<int> tagIds)
        {
            var note = await _db.Notes
                .Include(n => n.Workspace)
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
                throw new KeyNotFoundException("笔记不存在。");

            bool canEdit = note.Workspace.OwnerUserId == userId ||
                           await _db.WorkspaceMembers.AnyAsync(m =>
                                m.WorkspaceId == note.WorkspaceId &&
                                m.UserId == userId &&
                                m.CanEdit);
            if (!canEdit)
                throw new UnauthorizedAccessException("无权修改标签。");

            var old = _db.NoteTags.Where(nt => nt.NoteId == noteId);
            _db.NoteTags.RemoveRange(old);

            foreach (var tagId in tagIds)
            {
                _db.NoteTags.Add(new NoteTag
                {
                    NoteId = noteId,
                    TagId = tagId
                });
            }

            await _db.SaveChangesAsync();
        }


        // =========================================
        // 软删除
        // =========================================
        public async Task<int> SoftDeleteAsync(IEnumerable<int> noteIds, int userId)
        {
            var notes = await _db.Notes
                .Include(n => n.Workspace)
                .Where(n => noteIds.Contains(n.Id))
                .ToListAsync();

            foreach (var note in notes)
            {
                bool canEdit =
                      note.Workspace.OwnerUserId == userId ||
                      await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == note.WorkspaceId && m.UserId == userId && m.CanEdit);

                if (!canEdit || note.IsDeleted)
                    continue;

                note.IsDeleted = true;
                note.DeletedTime = DateTime.UtcNow;
                note.LastUpdateTime = DateTime.UtcNow;
            }

            return await _db.SaveChangesAsync();
        }
    }
}
