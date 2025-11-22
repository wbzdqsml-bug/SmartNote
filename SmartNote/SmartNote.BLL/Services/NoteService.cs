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

        /// <summary>
        /// 根据笔记类型生成默认内容模板
        /// </summary>
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

        /// <summary>
        /// 获取用户可访问的工作区 Id 列表（自己创建 + 成员）
        /// </summary>
        private async Task<List<int>> GetAccessibleWorkspaceIdsAsync(int userId)
        {
            return await _db.WorkspaceMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.WorkspaceId)
                .Union(
                    _db.Workspaces
                        .Where(w => w.OwnerUserId == userId)
                        .Select(w => w.Id)
                )
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 将 Note 实体映射为 NoteViewDto（带分类 + 标签）
        /// </summary>
        private static NoteViewDto MapToDto(Note n)
        {
            return new NoteViewDto
            {
                Id = n.Id,
                Title = n.Title,
                Type = n.Type,
                ContentJson = n.ContentJson,
                WorkspaceId = n.WorkspaceId,
                CreateTime = n.CreateTime,
                LastUpdateTime = n.LastUpdateTime,
                IsDeleted = n.IsDeleted,
                DeletedTime = n.DeletedTime,

                CategoryId = n.CategoryId,
                CategoryName = n.Category?.Name,
                CategoryColor = n.Category?.Color,

                Tags = n.NoteTags?.Select(nt => new TagDto
                {
                    Id = nt.Tag.Id,
                    Name = nt.Tag.Name,
                    Color = nt.Tag.Color
                }).ToList() ?? new List<TagDto>()
            };
        }

        /// <summary>
        /// 获取当前用户所有可访问笔记（完整加载 分类 + 标签）
        /// </summary>
        public async Task<IEnumerable<NoteViewDto>> GetUserNotesAsync(int userId)
        {
            var workspaceIds = await GetAccessibleWorkspaceIdsAsync(userId);

            var notes = await _db.Notes
                .Include(n => n.Category)
                .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
                .Where(n => workspaceIds.Contains(n.WorkspaceId) && !n.IsDeleted)
                .OrderByDescending(n => n.LastUpdateTime)
                .ToListAsync();

            return notes.Select(MapToDto);
        }

        /// <summary>
        /// 获取单条笔记详情（含分类 + 标签）
        /// </summary>
        public async Task<NoteViewDto?> GetNoteByIdAsync(int userId, int noteId)
        {
            var workspaceIds = await GetAccessibleWorkspaceIdsAsync(userId);

            var note = await _db.Notes
                .Include(n => n.Category)
                .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
                .Where(n => !n.IsDeleted &&
                            n.Id == noteId &&
                            workspaceIds.Contains(n.WorkspaceId))
                .FirstOrDefaultAsync();

            return note == null ? null : MapToDto(note);
        }

        /// <summary>
        /// 按分类 / 标签筛选笔记
        /// </summary>
        public async Task<IEnumerable<NoteViewDto>> FilterNotesAsync(
            int userId,
            int? categoryId,
            IReadOnlyList<int>? tagIds)
        {
            var workspaceIds = await GetAccessibleWorkspaceIdsAsync(userId);

            var query = _db.Notes
                .Include(n => n.Category)
                .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
                .Where(n => workspaceIds.Contains(n.WorkspaceId) && !n.IsDeleted)
                .AsQueryable();

            // 按分类
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(n => n.CategoryId == categoryId.Value);
            }

            // 按标签（AND：必须同时包含 tagIds 中所有标签）
            if (tagIds != null && tagIds.Count > 0)
            {
                foreach (var tagId in tagIds)
                {
                    var tid = tagId;
                    query = query.Where(n => n.NoteTags.Any(nt => nt.TagId == tid));
                }
            }

            var list = await query
                .OrderByDescending(n => n.LastUpdateTime)
                .ToListAsync();

            return list.Select(MapToDto);
        }

        /// <summary>
        /// 创建笔记（可带初始分类和标签）
        /// </summary>
        public async Task<int> CreateNoteAsync(int userId, NoteCreateDto dto)
        {
            // 权限：工作区拥有者或具有编辑权限的成员
            bool canCreate = await _db.Workspaces.AnyAsync(w => w.Id == dto.WorkspaceId && w.OwnerUserId == userId)
                || await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == dto.WorkspaceId &&
                                                            m.UserId == userId &&
                                                            m.CanEdit);

            if (!canCreate)
                throw new BusinessException("无权在该工作区创建笔记。");

            var note = new Note
            {
                WorkspaceId = dto.WorkspaceId,
                Title = dto.Title,
                Type = dto.Type,
                ContentJson = GetDefaultContentJson(dto.Type),
                CategoryId = dto.CategoryId,
                CreateTime = DateTime.UtcNow,
                LastUpdateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            _db.Notes.Add(note);
            await _db.SaveChangesAsync();

            // 初始标签
            if (dto.TagIds != null && dto.TagIds.Count > 0)
            {
                foreach (var tagId in dto.TagIds.Distinct())
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

        /// <summary>
        /// 更新笔记内容 / 标题 / 分类（不负责标签）
        /// </summary>
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

            // ⭐⭐⭐ 允许清空分类（null）
            if (dto.CategoryId != null || dto.CategoryId == null)
                note.CategoryId = dto.CategoryId;

            note.LastUpdateTime = DateTime.UtcNow;

            return await _db.SaveChangesAsync();
        }


        /// <summary>
        /// 覆盖式更新某笔记的标签（编辑页用）
        /// </summary>
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
                throw new BusinessException("无权修改该笔记的标签。");

            tagIds ??= new List<int>();

            // 清空旧关系
            var oldRelations = await _db.NoteTags
                .Where(nt => nt.NoteId == noteId)
                .ToListAsync();

            _db.NoteTags.RemoveRange(oldRelations);

            // 新建关系
            foreach (var tagId in tagIds.Distinct())
            {
                _db.NoteTags.Add(new NoteTag
                {
                    NoteId = noteId,
                    TagId = tagId
                });
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// 批量软删除笔记
        /// </summary>
        public async Task<int> SoftDeleteAsync(IEnumerable<int> noteIds, int userId)
        {
            var ids = noteIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return 0;

            var notes = await _db.Notes
                .Include(n => n.Workspace)
                .Where(n => ids.Contains(n.Id))
                .ToListAsync();

            var now = DateTime.UtcNow;
            int affected = 0;

            foreach (var note in notes)
            {
                bool canEdit = note.Workspace.OwnerUserId == userId ||
                    await _db.WorkspaceMembers.AnyAsync(m =>
                        m.WorkspaceId == note.WorkspaceId &&
                        m.UserId == userId &&
                        m.CanEdit);

                if (!canEdit || note.IsDeleted)
                    continue;

                note.IsDeleted = true;
                note.DeletedTime = now;
                note.LastUpdateTime = now;
                affected++;
            }

            if (affected == 0)
                return 0;

            await _db.SaveChangesAsync();
            return affected;
        }
    }
}
