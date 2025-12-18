using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface IFriendService
    {
        Task<List<FriendDto>> GetMyFriendsAsync(int userId);
        Task SendRequestAsync(int userId, string targetUsername);
        Task<List<FriendRequestDto>> GetRequestsAsync(int userId);
        Task HandleRequestAsync(int userId, int requestId, string action);
    }
}