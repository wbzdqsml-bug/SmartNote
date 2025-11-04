using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface IWorkspaceInvitationService : ICollaborationServiceBase<WorkspaceInvitationViewDto>
    {
        Task SendInvitationAsync(int workspaceId, int inviterUserId, WorkspaceInvitationSendDto dto);
        Task RevokeInvitationAsync(int invitationId, int inviterUserId);
    }
}
