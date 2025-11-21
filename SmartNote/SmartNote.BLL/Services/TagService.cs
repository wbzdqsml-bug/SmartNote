using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;


namespace SmartNote.BLL.Services
{
    public class TagService : ITagService
    {
        private readonly ApplicationDbContext _db;

        public TagService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<TagDto>> GetUserTagsAsync(int userId)
        {
            var list = await _db.Tags
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Name)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Color = t.Color
                })
                .ToListAsync();

            return list;
        }

        public async Task<int> CreateTagAsync(int userId, TagCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                throw new BusinessException("标签名称不能为空。");

            var exists = await _db.Tags.AnyAsync(t => t.UserId == userId && t.Name == req.Name);
            if (exists)
                throw new BusinessException("已存在同名标签。");

            var tag = new Tag
            {
                UserId = userId,
                Name = req.Name.Trim(),
                Color = req.Color,
                CreateTime = DateTime.UtcNow
            };

            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();
            return tag.Id;
        }

        public async Task UpdateTagAsync(int userId, int tagId, TagUpdateRequest req)
        {
            var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == tagId && t.UserId == userId);
            if (tag == null)
                throw new BusinessException("标签不存在。");

            if (string.IsNullOrWhiteSpace(req.Name))
                throw new BusinessException("标签名称不能为空。");

            var exists = await _db.Tags.AnyAsync(t =>
                t.UserId == userId && t.Id != tagId && t.Name == req.Name);
            if (exists)
                throw new BusinessException("已存在同名标签。");

            tag.Name = req.Name.Trim();
            tag.Color = req.Color;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteTagAsync(int userId, int tagId)
        {
            var tag = await _db.Tags
                .Include(t => t.NoteTags)
                .FirstOrDefaultAsync(t => t.Id == tagId && t.UserId == userId);

            if (tag == null)
                throw new BusinessException("标签不存在。");

            _db.NoteTags.RemoveRange(tag.NoteTags);
            _db.Tags.Remove(tag);

            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<TagDto>> GetNoteTagsAsync(int userId, int noteId)
        {
            // 验证该笔记属于当前用户可访问的工作区（和 NoteService 类似）
            var note = await _db.Notes
                .Include(n => n.Workspace)
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
                throw new BusinessException("笔记不存在。");

            // 只要是所在工作区的成员即可查看标签
            var canView = note.Workspace.OwnerUserId == userId ||
                          await _db.WorkspaceMembers.AnyAsync(m =>
                              m.WorkspaceId == note.WorkspaceId && m.UserId == userId);
            if (!canView)
                throw new BusinessException("无权查看该笔记的标签。");

            var list = await _db.NoteTags
                .Where(nt => nt.NoteId == noteId)
                .Select(nt => nt.Tag)
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Color = t.Color
                })
                .ToListAsync();

            return list;
        }

        public async Task UpdateNoteTagsAsync(int userId, int noteId, IReadOnlyList<int> tagIds)
        {
            var note = await _db.Notes
                .Include(n => n.Workspace)
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
                throw new BusinessException("笔记不存在。");

            // 只有工作区 Owner 或具有编辑权限的成员可以修改标签
            var canEdit = note.Workspace.OwnerUserId == userId ||
                          await _db.WorkspaceMembers.AnyAsync(m =>
                              m.WorkspaceId == note.WorkspaceId &&
                              m.UserId == userId &&
                              m.CanEdit);
            if (!canEdit)
                throw new BusinessException("无权修改该笔记的标签。");

            // 只允许绑定当前用户创建的标签
            var validTags = await _db.Tags
                .Where(t => t.UserId == userId && tagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            // 先删除原有关系
            var oldRelations = await _db.NoteTags
                .Where(nt => nt.NoteId == noteId)
                .ToListAsync();
            _db.NoteTags.RemoveRange(oldRelations);

            // 再添加新关系
            var newRelations = validTags
                .Distinct()
                .Select(tagId => new NoteTag
                {
                    NoteId = noteId,
                    TagId = tagId
                });

            await _db.NoteTags.AddRangeAsync(newRelations);
            await _db.SaveChangesAsync();
        }
    }
}
