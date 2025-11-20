using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Domain.Exceptions;
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
            var workspace = await _db.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId);
            if (workspace == null) throw new KeyNotFoundException("工作区不存在。");

            var isOwner = workspace.OwnerUserId == inviterUserId;
            var member = await _db.WorkspaceMembers.FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == inviterUserId);

            var canShare = isOwner || (member != null && member.CanShare);
            if (!canShare)
                throw new PermissionDeniedException("无权邀请成员进入该工作区。");

            var invitee = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.InviteeUsername);
            if (invitee == null) throw new KeyNotFoundException("被邀请用户不存在。");
            if (invitee.Id == inviterUserId) throw new BusinessException("不能邀请自己。");

            var alreadyMember = await _db.WorkspaceMembers
                .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == invitee.Id);
            if (alreadyMember)
                throw new BusinessException("该用户已是工作区成员。");

            var existingPending = await _db.WorkspaceInvitations
                .AnyAsync(i => i.WorkspaceId == workspaceId
                            && i.InviteeUserId == invitee.Id
                            && i.Status == InvitationStatus.Pending);
            if (existingPending)
                throw new BusinessException("该用户已有待处理邀请。");

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
            var inv = await _db.WorkspaceInvitations.FirstOrDefaultAsync(i => i.Id == invitationId);
            if (inv == null) throw new KeyNotFoundException("邀请不存在。");

            if (inv.InviteeUserId != userId)
                throw new PermissionDeniedException("无权操作该邀请。");

            if (inv.Status != InvitationStatus.Pending)
                throw new BusinessException("邀请已处理或被撤销。");

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
                existing.CanEdit = inv.CanEdit;
                existing.CanShare = inv.CanShare;
            }

            inv.Status = InvitationStatus.Accepted;
            inv.RespondedTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task RejectInvitationAsync(int invitationId, int userId)
        {
            var inv = await _db.WorkspaceInvitations.FirstOrDefaultAsync(i => i.Id == invitationId);
            if (inv == null) throw new KeyNotFoundException("邀请不存在。");

            if (inv.InviteeUserId != userId)
                throw new PermissionDeniedException("无权操作该邀请。");

            if (inv.Status != InvitationStatus.Pending)
                throw new BusinessException("邀请已处理或被撤销。");

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

            var isOwner = inv.Workspace.OwnerUserId == inviterUserId;

            if (!isOwner && inv.InviterUserId != inviterUserId)
                throw new PermissionDeniedException("无权撤销该邀请。");

            if (inv.Status != InvitationStatus.Pending)
                throw new BusinessException("非待处理邀请不可撤销。");

            inv.Status = InvitationStatus.Revoked;
            inv.RespondedTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
