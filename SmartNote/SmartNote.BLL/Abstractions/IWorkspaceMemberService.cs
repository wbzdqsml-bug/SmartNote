using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface IWorkspaceMemberService
    {
        /// <summary>
        /// 获取工作区成员列表（任何成员都可查看）
        /// </summary>
        Task<IReadOnlyList<MemberDto>> GetMembersAsync(int workspaceId, int userId);

        /// <summary>
        /// 邀请成员（Owner 或 CanShare = true）
        /// </summary>
        Task InviteMemberAsync(int workspaceId, int operatorUserId, string username);

        /// <summary>
        /// 移除成员（仅 Owner）
        /// </summary>
        Task RemoveMemberAsync(int workspaceId, int operatorUserId, int targetUserId);

        /// <summary>
        /// 更新成员权限（仅 Owner）
        /// </summary>
        Task UpdatePermissionsAsync(int workspaceId, int operatorUserId, int targetUserId, bool canEdit, bool canShare);

        /// <summary>
        /// 退出工作区（Owner 不允许退出）
        /// </summary>
        Task LeaveWorkspaceAsync(int workspaceId, int userId);
    }
}
