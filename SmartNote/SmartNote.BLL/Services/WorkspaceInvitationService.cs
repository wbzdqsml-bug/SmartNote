using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Services
{
    public class WorkspaceInvitationService : IWorkspaceInvitationService
    {
        private readonly ApplicationDbContext _db;

        public WorkspaceInvitationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<WorkspaceInvitationViewDto>> GetUserInvitationsAsync(int userId)
        {
            // 我收到的所有邀请（按时间倒序）
            var list = await (from inv in _db.WorkspaceInvitations
                              join w in _db.Workspaces on inv.WorkspaceId equals w.Id
                              join u in _db.Users on inv.InviterUserId equals u.Id
                              where inv.InviteeUserId == userId
                              orderby inv.CreatedTime descending
                              select new WorkspaceInvitationViewDto
                              {
                                  InvitationId = inv.Id,
                                  WorkspaceId = inv.WorkspaceId,
                                  WorkspaceName = w.Name,
                                  InviterUserId = inv.InviterUserId,
                                  InviterUsername = u.Username,
                                  CanEdit = inv.CanEdit,
                                  CanShare = inv.CanShare,
                                  Status = inv.Status.ToString(),
                                  Message = inv.Message,
                                  CreatedTime = inv.CreatedTime,
                                  RespondedTime = inv.RespondedTime
                              }).ToListAsync();

            return list;
        }

        public async Task SendInvitationAsync(int workspaceId, int inviterUserId, WorkspaceInvitationSendDto dto)
        {
            // 校验工作区与发起人权限（Owner 或具备分享权限的成员）
            var workspace = await _db.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId);
            if (workspace == null) throw new KeyNotFoundException("工作区不存在。");

            var isOwner = workspace.OwnerUserId == inviterUserId;
            var member = await _db.WorkspaceMembers
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == inviterUserId);

            var canShare = isOwner || (member != null && member.CanShare);
            if (!canShare) throw new UnauthorizedAccessException("无权邀请成员进入该工作区。");

            // 被邀请人
            var invitee = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.InviteeUsername);
            if (invitee == null) throw new KeyNotFoundException("被邀请用户不存在。");
            if (invitee.Id == inviterUserId) throw new InvalidOperationException("不能邀请自己。");

            // 已经是成员则无需邀请
            var alreadyMember = await _db.WorkspaceMembers
                .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == invitee.Id);
            if (alreadyMember) throw new InvalidOperationException("该用户已是工作区成员。");

            // 是否已有待处理邀请（避免重复）
            var existingPending = await _db.WorkspaceInvitations
                .AnyAsync(i => i.WorkspaceId == workspaceId
                            && i.InviteeUserId == invitee.Id
                            && i.Status == InvitationStatus.Pending);
            if (existingPending) throw new InvalidOperationException("该用户已有待处理邀请。");

            var invitation = new WorkspaceInvitation
            {
                WorkspaceId = workspaceId,
                InviterUserId = inviterUserId,
                InviteeUserId = invitee.Id,
                CanEdit = dto.CanEdit,
                CanShare = dto.CanShare,
                Message = dto.Message,
                Status = InvitationStatus.Pending,
                CreatedTime = DateTime.UtcNow
            };

            _db.WorkspaceInvitations.Add(invitation);
            await _db.SaveChangesAsync();
        }

        public async Task AcceptInvitationAsync(int invitationId, int userId)
        {
            var inv = await _db.WorkspaceInvitations
                               .FirstOrDefaultAsync(i => i.Id == invitationId);
            if (inv == null) throw new KeyNotFoundException("邀请不存在。");
            if (inv.InviteeUserId != userId) throw new UnauthorizedAccessException("无权操作该邀请。");
            if (inv.Status != InvitationStatus.Pending) throw new InvalidOperationException("邀请已处理或被撤销。");

            // 加入成员表（若已存在则更新权限）
            var existing = await _db.WorkspaceMembers
                                    .FirstOrDefaultAsync(m => m.WorkspaceId == inv.WorkspaceId && m.UserId == userId);

            if (existing == null)
            {
                _db.WorkspaceMembers.Add(new WorkspaceMember
                {
                    WorkspaceId = inv.WorkspaceId,
                    UserId = userId,
                    CanEdit = inv.CanEdit,
                    CanShare = inv.CanShare,
                    JoinTime = DateTime.UtcNow
                });
            }
            else
            {
                // 已存在成员可按邀请同步权限（具体策略可自定）
                existing.CanEdit = inv.CanEdit;
                existing.CanShare = inv.CanShare;
            }

            inv.Status = InvitationStatus.Accepted;
            inv.RespondedTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task RejectInvitationAsync(int invitationId, int userId)
        {
            var inv = await _db.WorkspaceInvitations
                               .FirstOrDefaultAsync(i => i.Id == invitationId);
            if (inv == null) throw new KeyNotFoundException("邀请不存在。");
            if (inv.InviteeUserId != userId) throw new UnauthorizedAccessException("无权操作该邀请。");
            if (inv.Status != InvitationStatus.Pending) throw new InvalidOperationException("邀请已处理或被撤销。");

            inv.Status = InvitationStatus.Rejected;
            inv.RespondedTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task RevokeInvitationAsync(int invitationId, int inviterUserId)
        {
            var inv = await _db.WorkspaceInvitations
                               .Include(i => i.Workspace)
                               .FirstOrDefaultAsync(i => i.Id == invitationId);
            if (inv == null) throw new KeyNotFoundException("邀请不存在。");

            // 只有发起者或工作区拥有者可撤销
            var isOwner = inv.Workspace.OwnerUserId == inviterUserId;
            if (!isOwner && inv.InviterUserId != inviterUserId)
                throw new UnauthorizedAccessException("无权撤销该邀请。");

            if (inv.Status != InvitationStatus.Pending)
                throw new InvalidOperationException("非待处理邀请不可撤销。");

            inv.Status = InvitationStatus.Revoked;
            inv.RespondedTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
