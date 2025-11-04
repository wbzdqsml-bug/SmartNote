using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Services
{
    public class WorkspaceMemberService : IWorkspaceMemberService
    {
        private readonly ApplicationDbContext _db;
        private const int MaxMembersPerWorkspace = 6; // 规格要求：最多 6 人

        public WorkspaceMemberService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<MemberDto>> GetMembersAsync(int workspaceId, int userId)
        {
            // 只要是成员即可查看
            var isMember = await _db.WorkspaceMembers
                .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);
            if (!isMember)
                throw new BusinessException("你不是该工作区成员，无法查看成员列表。");

            var q = from m in _db.WorkspaceMembers.AsNoTracking()
                    join u in _db.Users.AsNoTracking() on m.UserId equals u.Id
                    where m.WorkspaceId == workspaceId
                    orderby m.JoinTime
                    select new MemberDto
                    {
                        UserId = u.Id,
                        Username = u.Username,
                        Role = m.Role.ToString(),
                        CanEdit = m.CanEdit,
                        CanShare = m.CanShare,
                        JoinTime = m.JoinTime
                    };

            var list = await q.ToListAsync();
            return list;
        }

        public async Task InviteMemberAsync(int workspaceId, int operatorUserId, string username)
        {
            var workspace = await _db.Workspaces
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workspaceId);
            if (workspace == null)
                throw new BusinessException("工作区不存在。");

            // 邀请权限：Owner 或具有 CanShare 的成员
            if (!await HasInvitePermissionAsync(workspace, operatorUserId))
                throw new BusinessException("你没有邀请成员的权限。");

            var invitee = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (invitee == null)
                throw new BusinessException("被邀请用户不存在。");

            // 已是成员？
            var exists = await _db.WorkspaceMembers
                .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == invitee.Id);
            if (exists)
                throw new BusinessException("该用户已在工作区中。");

            // 人数限制（最多 6 人）
            var currentCount = await _db.WorkspaceMembers
                .CountAsync(m => m.WorkspaceId == workspaceId);
            if (currentCount >= MaxMembersPerWorkspace)
                throw new BusinessException("工作区成员已达上限（最多 6 人）。");

            _db.WorkspaceMembers.Add(new WorkspaceMember
            {
                WorkspaceId = workspaceId,
                UserId = invitee.Id,
                Role = WorkspaceRole.Member,
                JoinTime = DateTime.UtcNow,
                CanEdit = false,
                CanShare = false
            });

            await _db.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(int workspaceId, int operatorUserId, int targetUserId)
        {
            var workspace = await _db.Workspaces
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workspaceId);
            if (workspace == null)
                throw new BusinessException("工作区不存在。");

            // 只有 Owner 可移除成员
            if (workspace.OwnerUserId != operatorUserId)
                throw new BusinessException("只有工作区拥有者可以移除成员。");

            if (targetUserId == workspace.OwnerUserId)
                throw new BusinessException("无法移除工作区拥有者。");

            var relation = await _db.WorkspaceMembers
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == targetUserId);
            if (relation == null)
                throw new BusinessException("该成员不在工作区中。");

            _db.WorkspaceMembers.Remove(relation);
            await _db.SaveChangesAsync();
        }

        public async Task UpdatePermissionsAsync(int workspaceId, int operatorUserId, int targetUserId, bool canEdit, bool canShare)
        {
            var workspace = await _db.Workspaces
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workspaceId);
            if (workspace == null)
                throw new BusinessException("工作区不存在。");

            // 只有 Owner 可修改权限
            if (workspace.OwnerUserId != operatorUserId)
                throw new BusinessException("只有工作区拥有者可以修改成员权限。");

            if (targetUserId == workspace.OwnerUserId)
                throw new BusinessException("不能修改拥有者自身的权限。");

            var relation = await _db.WorkspaceMembers
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == targetUserId);
            if (relation == null)
                throw new BusinessException("该成员不在工作区中。");

            relation.CanEdit = canEdit;
            relation.CanShare = canShare;

            await _db.SaveChangesAsync();
        }

        public async Task LeaveWorkspaceAsync(int workspaceId, int userId)
        {
            var workspace = await _db.Workspaces
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workspaceId);
            if (workspace == null)
                throw new BusinessException("工作区不存在。");

            if (workspace.OwnerUserId == userId)
                throw new BusinessException("工作区拥有者无法直接退出，请先转移拥有者或删除工作区。");

            var relation = await _db.WorkspaceMembers
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);
            if (relation == null)
                throw new BusinessException("你不是该工作区成员。");

            _db.WorkspaceMembers.Remove(relation);
            await _db.SaveChangesAsync();
        }

        private async Task<bool> HasInvitePermissionAsync(Workspace workspace, int operatorUserId)
        {
            if (workspace.OwnerUserId == operatorUserId)
                return true;

            var rel = await _db.WorkspaceMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspace.Id && m.UserId == operatorUserId);

            return rel != null && (rel.CanShare || rel.Role == WorkspaceRole.Admin);
        }
    }
}
