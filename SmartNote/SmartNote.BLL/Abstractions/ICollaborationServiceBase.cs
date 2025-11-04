namespace SmartNote.BLL.Abstractions
{
    // 预留的“高级接口层”，未来 Note/Resource 协作都可复用该形态
    public interface ICollaborationServiceBase<TInvitationViewDto>
    {
        Task<IEnumerable<TInvitationViewDto>> GetUserInvitationsAsync(int userId);
        Task AcceptInvitationAsync(int invitationId, int userId);
        Task RejectInvitationAsync(int invitationId, int userId);
    }
}
