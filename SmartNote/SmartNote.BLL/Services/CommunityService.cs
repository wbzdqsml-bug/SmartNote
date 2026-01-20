using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.Common.Helpers;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Services
{
    public class CommunityService : ICommunityService
    {
        private readonly ApplicationDbContext _db;

        public CommunityService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PublicContentPageDto> GetPublishedPageAsync(string? keyword, int? contentType, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

            var query = _db.PublicContents
                .Include(pc => pc.Note)
                .Include(pc => pc.Stats)
                .Include(pc => pc.AuthorUser)
                .Where(pc => pc.Status == PublicContentStatus.Published)
                .AsQueryable();

            if (contentType.HasValue && Enum.IsDefined(typeof(PublicContentType), contentType.Value))
            {
                query = query.Where(pc => pc.ContentType == (PublicContentType)contentType.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(pc => (pc.TitleSnapshot ?? pc.Note.Title).Contains(keyword));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(pc => pc.PublishedAt ?? pc.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PublicContentPageDto
            {
                TotalCount = total,
                Items = items.Select(MapToListItem).ToList()
            };
        }

        public async Task<PublicContentDetailDto?> GetPublicContentDetailAsync(int publicContentId, bool increaseView)
        {
            var content = await _db.PublicContents
                .Include(pc => pc.Note)
                .Include(pc => pc.Stats)
                .Include(pc => pc.AuthorUser)
                .Where(pc => pc.Id == publicContentId && pc.Status == PublicContentStatus.Published)
                .FirstOrDefaultAsync();

            if (content == null)
                return null;

            await EnsureStatsAsync(content);

            if (increaseView)
            {
                content.Stats.ViewCount += 1;
                await _db.SaveChangesAsync();
            }

            return MapToDetail(content);
        }

        public async Task<PublicContentPageDto> GetMyPublicContentsAsync(int userId, PublicContentStatus? status, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

            var query = _db.PublicContents
                .Include(pc => pc.Note)
                .Include(pc => pc.Stats)
                .Include(pc => pc.AuthorUser)
                .Where(pc => pc.AuthorUserId == userId)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(pc => pc.Status == status.Value);
            }

            var total = await query.CountAsync();
            var contents = await query
                .OrderByDescending(pc => pc.LastUpdateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PublicContentPageDto
            {
                TotalCount = total,
                Items = contents.Select(MapToListItem).ToList()
            };
        }

        public async Task<IEnumerable<PublicCommentDto>> GetCommentsAsync(int publicContentId)
        {
            var comments = await _db.PublicComments
                .Include(c => c.AuthorUser)
                .Where(c => c.PublicContentId == publicContentId)
                .OrderBy(c => c.CreateTime)
                .ToListAsync();

            var commentDtos = comments.Select(c => new PublicCommentDto
            {
                Id = c.Id,
                PublicContentId = c.PublicContentId,
                AuthorUserId = c.AuthorUserId,
                AuthorName = c.AuthorUser.Username,
                ParentId = c.ParentId,
                Content = c.Content,
                CreateTime = c.CreateTime
            }).ToList();

            return BuildCommentTree(commentDtos);
        }

        public async Task<PublicCommentDto> AddCommentAsync(int userId, PublicCommentCreateDto dto)
        {
            var contentExists = await _db.PublicContents
                .AnyAsync(pc => pc.Id == dto.PublicContentId && pc.Status == PublicContentStatus.Published);
            if (!contentExists)
                throw new BusinessException("内容不存在或未发布。");

            if (dto.ParentId.HasValue)
            {
                var parentExists = await _db.PublicComments
                    .AnyAsync(c => c.Id == dto.ParentId.Value && c.PublicContentId == dto.PublicContentId);
                if (!parentExists)
                    throw new BusinessException("父评论不存在。");
            }

            var comment = new PublicComment
            {
                PublicContentId = dto.PublicContentId,
                AuthorUserId = userId,
                ParentId = dto.ParentId,
                Content = dto.Content
            };

            _db.PublicComments.Add(comment);
            await _db.SaveChangesAsync();

            var author = await _db.Users.FirstAsync(u => u.Id == userId);
            return new PublicCommentDto
            {
                Id = comment.Id,
                PublicContentId = comment.PublicContentId,
                AuthorUserId = comment.AuthorUserId,
                AuthorName = author.Username,
                ParentId = comment.ParentId,
                Content = comment.Content,
                CreateTime = comment.CreateTime
            };
        }

        public async Task DeleteCommentAsync(int userId, int commentId)
        {
            var comment = await _db.PublicComments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return;

            if (comment.AuthorUserId != userId)
                throw new BusinessException("无权删除该评论。");

            _db.PublicComments.Remove(comment);
            await _db.SaveChangesAsync();
        }

        public async Task<int> CloneAsync(int userId, PublicContentCloneRequest request)
        {
            var content = await _db.PublicContents
                .Include(pc => pc.Note)
                .Include(pc => pc.Note.NoteTags)
                .Include(pc => pc.Note.NoteTags).ThenInclude(nt => nt.Tag)
                .Include(pc => pc.Note.NoteAttachments)
                .Include(pc => pc.Stats)
                .FirstOrDefaultAsync(pc => pc.Id == request.PublicContentId && pc.Status == PublicContentStatus.Published);

            if (content == null)
                throw new BusinessException("内容不存在或未发布。");

            var workspaceIds = await GetAccessibleWorkspaceIdsAsync(userId);
            if (!workspaceIds.Contains(request.WorkspaceId))
                throw new BusinessException("无权限克隆到该工作区。");

            var note = content.Note;
            var cloneNote = new Note
            {
                WorkspaceId = request.WorkspaceId,
                Title = string.IsNullOrWhiteSpace(request.Title) ? note.Title : request.Title!,
                ContentJson = content.ContentSnapshotJson ?? note.ContentJson,
                Type = note.Type,
                CategoryId = note.CategoryId
            };

            _db.Notes.Add(cloneNote);
            await _db.SaveChangesAsync();

            if (note.NoteTags.Any())
            {
                var cloneTags = note.NoteTags.Select(nt => new NoteTag
                {
                    NoteId = cloneNote.Id,
                    TagId = nt.TagId
                });
                _db.NoteTags.AddRange(cloneTags);
            }

            if (note.NoteAttachments.Any())
            {
                var cloneAttachments = note.NoteAttachments.Select(att => new NoteAttachment
                {
                    NoteId = cloneNote.Id,
                    UploaderUserId = userId,
                    OriginalFileName = att.OriginalFileName,
                    ContentType = att.ContentType,
                    Size = att.Size,
                    StoragePath = att.StoragePath
                });
                _db.NoteAttachments.AddRange(cloneAttachments);
            }

            await EnsureStatsAsync(content);
            content.Stats.CloneCount += 1;
            await _db.SaveChangesAsync();

            return cloneNote.Id;
        }

        public async Task ToggleReactionAsync(int userId, PublicContentReactionRequest request)
        {
            var content = await _db.PublicContents
                .Include(pc => pc.Stats)
                .FirstOrDefaultAsync(pc => pc.Id == request.PublicContentId && pc.Status == PublicContentStatus.Published);
            if (content == null)
                throw new BusinessException("内容不存在或未发布。");

            await EnsureStatsAsync(content);

            var reaction = await _db.PublicContentReactions
                .FirstOrDefaultAsync(r => r.PublicContentId == request.PublicContentId && r.UserId == userId);

            var previousLiked = reaction?.IsLiked ?? false;
            var previousFavorite = reaction?.IsFavorite ?? false;

            if (reaction == null)
            {
                reaction = new PublicContentReaction
                {
                    PublicContentId = request.PublicContentId,
                    UserId = userId,
                    IsLiked = request.IsLiked,
                    IsFavorite = request.IsFavorite
                };
                _db.PublicContentReactions.Add(reaction);
            }
            else
            {
                reaction.IsLiked = request.IsLiked;
                reaction.IsFavorite = request.IsFavorite;
            }

            content.Stats.LikeCount += CalculateDelta(previousLiked, request.IsLiked);
            content.Stats.FavoriteCount += CalculateDelta(previousFavorite, request.IsFavorite);

            await _db.SaveChangesAsync();
        }

        public async Task<int> PublishAsync(int userId, PublicContentPublishRequest request)
        {
            var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == request.NoteId);
            if (note == null)
                throw new BusinessException("笔记不存在。");

            await EnsureWorkspaceShareAccessAsync(userId, note.WorkspaceId);

            var content = await _db.PublicContents
                .Include(pc => pc.Stats)
                .FirstOrDefaultAsync(pc => pc.NoteId == request.NoteId && pc.AuthorUserId == userId);

            if (content == null)
            {
                content = new PublicContent
                {
                    NoteId = request.NoteId,
                    AuthorUserId = userId
                };
                _db.PublicContents.Add(content);
            }

            content.ContentType = request.ContentType;
            content.Status = PublicContentStatus.Published;
            content.PublishedAt ??= DateTime.UtcNow;
            content.TitleSnapshot = string.IsNullOrWhiteSpace(request.TitleSnapshot) ? note.Title : request.TitleSnapshot;
            content.ContentSnapshotJson = string.IsNullOrWhiteSpace(request.ContentSnapshotJson) ? note.ContentJson : request.ContentSnapshotJson;
            content.LastUpdateTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await EnsureStatsAsync(content);

            return content.Id;
        }

        public async Task UpdateStatusAsync(int userId, PublicContentStatusUpdateRequest request)
        {
            var content = await _db.PublicContents.FirstOrDefaultAsync(pc => pc.Id == request.PublicContentId);
            if (content == null)
                throw new BusinessException("内容不存在。");

            if (content.AuthorUserId != userId)
                throw new BusinessException("无权修改该内容。");

            content.Status = request.Status;
            if (content.Status == PublicContentStatus.Published && content.PublishedAt == null)
            {
                content.PublishedAt = DateTime.UtcNow;
            }
            content.LastUpdateTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        private static int CalculateDelta(bool previous, bool current)
        {
            if (previous == current)
                return 0;
            return current ? 1 : -1;
        }

        private async Task EnsureStatsAsync(PublicContent content)
        {
            if (content.Stats != null)
                return;

            var stats = await _db.PublicContentStats
                .FirstOrDefaultAsync(s => s.PublicContentId == content.Id);
            if (stats == null)
            {
                stats = new PublicContentStats
                {
                    PublicContentId = content.Id
                };
                _db.PublicContentStats.Add(stats);
                await _db.SaveChangesAsync();
            }

            content.Stats = stats;
        }

        private static PublicContentListItemDto MapToListItem(PublicContent content)
        {
            return new PublicContentListItemDto
            {
                Id = content.Id,
                NoteId = content.NoteId,
                Title = content.TitleSnapshot ?? content.Note.Title,
                Summary = BuildSummary(content),
                ContentType = content.ContentType,
                Status = content.Status,
                AuthorUserId = content.AuthorUserId,
                AuthorName = content.AuthorUser.Username,
                PublishedAt = content.PublishedAt,
                ViewCount = content.Stats?.ViewCount ?? 0,
                LikeCount = content.Stats?.LikeCount ?? 0,
                FavoriteCount = content.Stats?.FavoriteCount ?? 0,
                CloneCount = content.Stats?.CloneCount ?? 0
            };
        }

        private static PublicContentDetailDto MapToDetail(PublicContent content)
        {
            return new PublicContentDetailDto
            {
                Id = content.Id,
                NoteId = content.NoteId,
                Title = content.TitleSnapshot ?? content.Note.Title,
                ContentJson = content.ContentSnapshotJson ?? content.Note.ContentJson,
                ContentType = content.ContentType,
                Status = content.Status,
                AuthorUserId = content.AuthorUserId,
                AuthorName = content.AuthorUser.Username,
                PublishedAt = content.PublishedAt,
                ViewCount = content.Stats?.ViewCount ?? 0,
                LikeCount = content.Stats?.LikeCount ?? 0,
                FavoriteCount = content.Stats?.FavoriteCount ?? 0,
                CloneCount = content.Stats?.CloneCount ?? 0
            };
        }

        private static string BuildSummary(PublicContent content)
        {
            var source = content.ContentSnapshotJson;
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;
            return MarkdownHelper.BuildSummary(source, 100);
        }

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

        private async Task EnsureWorkspaceShareAccessAsync(int userId, int workspaceId)
        {
            var ownerAccess = await _db.Workspaces.AnyAsync(w => w.Id == workspaceId && w.OwnerUserId == userId);
            if (ownerAccess)
                return;

            var member = await _db.WorkspaceMembers.FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);
            if (member == null || !member.CanShare)
                throw new BusinessException("无权限发布该笔记。");
        }

        private static IEnumerable<PublicCommentDto> BuildCommentTree(List<PublicCommentDto> flat)
        {
            var lookup = flat.ToDictionary(c => c.Id, c => c);
            var roots = new List<PublicCommentDto>();

            foreach (var comment in flat)
            {
                if (comment.ParentId.HasValue && lookup.TryGetValue(comment.ParentId.Value, out var parent))
                {
                    parent.Replies.Add(comment);
                }
                else
                {
                    roots.Add(comment);
                }
            }

            return roots;
        }
    }
}
