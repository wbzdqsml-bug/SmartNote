using SmartNote.Domain.Entities.Enums;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface ICommunityService
    {
        Task<PublicContentPageDto> GetPublishedPageAsync(string? keyword, int? contentType, int page, int pageSize);
        Task<PublicContentDetailDto?> GetPublicContentDetailAsync(int publicContentId, bool increaseView);
        Task<PublicContentPageDto> GetMyPublicContentsAsync(int userId, PublicContentStatus? status, int page, int pageSize);
        Task<IEnumerable<PublicCommentDto>> GetCommentsAsync(int publicContentId);
        Task<PublicCommentDto> AddCommentAsync(int userId, PublicCommentCreateDto dto);
        Task DeleteCommentAsync(int userId, int commentId);
        Task<int> CloneAsync(int userId, PublicContentCloneRequest request);
        Task ToggleReactionAsync(int userId, PublicContentReactionRequest request);
        Task<int> PublishAsync(int userId, PublicContentPublishRequest request);
        Task UpdateStatusAsync(int userId, PublicContentStatusUpdateRequest request);
    }
}
