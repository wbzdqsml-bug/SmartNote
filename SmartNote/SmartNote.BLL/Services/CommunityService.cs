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
    /// <summary>
    /// 社区内容业务逻辑服务。
    /// </summary>
    public class CommunityService : ICommunityService
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// 初始化社区服务。
        /// </summary>
        public CommunityService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// 获取已发布内容分页数据。
        /// </summary>
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

        /// <summary>
        /// 获取已发布内容详情，可选择累加浏览数。
        /// </summary>
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

        /// <summary>
        /// 获取用户发布的内容列表，可按状态筛选。
        /// </summary>
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

        /// <summary>
        /// 获取指定内容的评论树。
        /// </summary>
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

        /// <summary>
        /// 新增评论。
        /// </summary>
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

        /// <summary>
        /// 删除评论及其子评论。
        /// </summary>
        public async Task DeleteCommentAsync(int userId, int commentId)
        {
            var comment = await _db.PublicComments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return;

            if (comment.AuthorUserId != userId)
                throw new BusinessException("无权删除该评论。");

            var allComments = await _db.PublicComments
                .Where(c => c.PublicContentId == comment.PublicContentId)
                .ToListAsync();

            var toDeleteIds = CollectCommentSubtreeIds(comment.Id, allComments);
            var toDelete = allComments.Where(c => toDeleteIds.Contains(c.Id)).ToList();

            _db.PublicComments.RemoveRange(toDelete);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// 克隆社区内容到指定工作区。
        /// </summary>
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

        /// <summary>
        /// 点赞/收藏切换并更新统计。
        /// </summary>
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

        /// <summary>
        /// 发布笔记到社区。
        /// </summary>
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
            else if (content.Status == PublicContentStatus.Banned)
            {
                throw new BusinessException("内容已被封禁，无法发布。");
            }

            content.ContentType = request.ContentType;
            content.Status = PublicContentStatus.Published;
            content.PublishedAt ??= DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.TitleSnapshot))
                content.TitleSnapshot = request.TitleSnapshot;
            else if (string.IsNullOrWhiteSpace(content.TitleSnapshot))
                content.TitleSnapshot = note.Title;

            if (!string.IsNullOrWhiteSpace(request.ContentSnapshotJson))
                content.ContentSnapshotJson = request.ContentSnapshotJson;
            else if (string.IsNullOrWhiteSpace(content.ContentSnapshotJson))
                content.ContentSnapshotJson = note.ContentJson;
            content.LastUpdateTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await EnsureStatsAsync(content);

            return content.Id;
        }

        /// <summary>
        /// 更新社区内容状态。
        /// </summary>
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

        /// <summary>
        /// 计算点赞/收藏的计数变化值。
        /// </summary>
        private static int CalculateDelta(bool previous, bool current)
        {
            if (previous == current)
                return 0;
            return current ? 1 : -1;
        }

        /// <summary>
        /// 确保内容统计记录存在。
        /// </summary>
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

        /// <summary>
        /// 将内容实体映射为列表项 DTO。
        /// </summary>
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

        /// <summary>
        /// 将内容实体映射为详情 DTO。
        /// </summary>
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

        /// <summary>
        /// 生成内容摘要文本。
        /// </summary>
        private static string BuildSummary(PublicContent content)
        {
            var source = content.ContentSnapshotJson;
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;
            return MarkdownHelper.BuildSummary(source, 100);
        }

        /// <summary>
        /// 获取用户可访问的工作区列表。
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
        /// 校验用户是否拥有工作区共享权限。
        /// </summary>
        private async Task EnsureWorkspaceShareAccessAsync(int userId, int workspaceId)
        {
            var ownerAccess = await _db.Workspaces.AnyAsync(w => w.Id == workspaceId && w.OwnerUserId == userId);
            if (ownerAccess)
                return;

            var member = await _db.WorkspaceMembers.FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);
            if (member == null || !member.CanShare)
                throw new BusinessException("无权限发布该笔记。");
        }

        /// <summary>
        /// 根据扁平评论列表构建评论树。
        /// </summary>
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

        /// <summary>
        /// 收集指定评论的子树评论 Id 集合。
        /// </summary>
        private static HashSet<int> CollectCommentSubtreeIds(int rootId, List<PublicComment> comments)
        {
            var lookup = comments.ToLookup(c => c.ParentId, c => c.Id);
            var result = new HashSet<int> { rootId };
            var queue = new Queue<int>();
            queue.Enqueue(rootId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var childId in lookup[current])
                {
                    if (result.Add(childId))
                        queue.Enqueue(childId);
                }
            }

            return result;
        }
    }
}
