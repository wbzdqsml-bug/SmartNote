using SmartNote.Domain.Entities.Enums;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    /// <summary>
    /// 社区内容相关服务接口。
    /// </summary>
    public interface ICommunityService
    {
        /// <summary>
        /// 获取已发布的社区内容分页列表。
        /// </summary>
        Task<PublicContentPageDto> GetPublishedPageAsync(string? keyword, int? contentType, int page, int pageSize);
        /// <summary>
        /// 获取指定社区内容详情，并可选择增加浏览数（可传入浏览者 Id 以过滤作者浏览）。
        /// </summary>
        Task<PublicContentDetailDto?> GetPublicContentDetailAsync(int publicContentId, bool increaseView, int? viewerUserId);
        /// <summary>
        /// 获取当前用户发布的内容列表，可按状态筛选。
        /// </summary>
        Task<PublicContentPageDto> GetMyPublicContentsAsync(int userId, PublicContentStatus? status, int page, int pageSize);
        /// <summary>
        /// 获取指定内容的评论树。
        /// </summary>
        Task<IEnumerable<PublicCommentDto>> GetCommentsAsync(int publicContentId);
        /// <summary>
        /// 新增评论。
        /// </summary>
        Task<PublicCommentDto> AddCommentAsync(int userId, PublicCommentCreateDto dto);
        /// <summary>
        /// 删除评论（包含子评论子树）。
        /// </summary>
        Task DeleteCommentAsync(int userId, int commentId);
        /// <summary>
        /// 克隆社区内容到指定工作区。
        /// </summary>
        Task<int> CloneAsync(int userId, PublicContentCloneRequest request);
        /// <summary>
        /// 切换点赞/收藏状态。
        /// </summary>
        Task ToggleReactionAsync(int userId, PublicContentReactionRequest request);
        /// <summary>
        /// 发布笔记到社区。
        /// </summary>
        Task<int> PublishAsync(int userId, PublicContentPublishRequest request);
        /// <summary>
        /// 更新内容状态（如发布/封禁）。
        /// </summary>
        Task UpdateStatusAsync(int userId, PublicContentStatusUpdateRequest request);
    }
}
