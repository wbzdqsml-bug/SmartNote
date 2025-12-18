namespace SmartNote.Shared.Dtos
{
    public record FriendDto(int FriendshipId, int FriendId, string FriendName, string? Avatar);
    public record FriendRequestDto(int RequestId, int RequesterId, string RequesterName, DateTime Time);
}