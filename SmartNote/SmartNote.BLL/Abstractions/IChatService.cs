using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface IChatService
    {
        Task<List<ChatMessageDto>> GetPrivateHistoryAsync(int userId, int friendId);
        Task<List<ChatMessageDto>> GetWorkspaceHistoryAsync(int userId, int workspaceId);
        Task<ChatMessageDto> SavePrivateMessageAsync(int senderId, int receiverId, string content);
        Task<ChatMessageDto> SaveWorkspaceMessageAsync(int senderId, int workspaceId, string content);
        Task<List<int>> GetUserWorkspaceIdsAsync(int userId);
    }
}