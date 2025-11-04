using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Shared.Dtos;
using SmartNote.Domain.Exceptions;


namespace SmartNote.BLL.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly ApplicationDbContext _db;

        public WorkspaceService(ApplicationDbContext db)
        {
            _db = db;
        }

        // 创建工作区
        public async Task<WorkspaceViewDto> CreateWorkspaceAsync(int userId, WorkspaceCreateDto dto)
        {
            var workspace = new Workspace
            {
                Name = dto.Name,
                Type = dto.Type,
                OwnerUserId = userId,
                CreateTime = DateTime.UtcNow
            };

            await _db.Workspaces.AddAsync(workspace);
            await _db.SaveChangesAsync();

            // 创建者自动加入成员表
            var member = new WorkspaceMember
            {
                WorkspaceId = workspace.Id,
                UserId = userId,
                Role = Domain.Entities.Enums.WorkspaceRole.Owner
            };
            await _db.WorkspaceMembers.AddAsync(member);
            await _db.SaveChangesAsync();

            return new WorkspaceViewDto
            {
                Id = workspace.Id,
                Name = workspace.Name,
                Type = workspace.Type,
                OwnerUserId = workspace.OwnerUserId,
                CreateTime = workspace.CreateTime,
                MemberCount = 1,
                NoteCount = 0
            };
        }

        // 获取用户参与的所有工作区
        public async Task<IEnumerable<WorkspaceViewDto>> GetUserWorkspacesAsync(int userId)
        {
            var query =
                from wm in _db.WorkspaceMembers
                join w in _db.Workspaces on wm.WorkspaceId equals w.Id
                where wm.UserId == userId
                select new WorkspaceViewDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Type = w.Type,
                    OwnerUserId = w.OwnerUserId,
                    CreateTime = w.CreateTime,
                    MemberCount = _db.WorkspaceMembers.Count(m => m.WorkspaceId == w.Id),
                    NoteCount = _db.Notes.Count(n => n.WorkspaceId == w.Id && !n.IsDeleted)
                };

            return await query.ToListAsync();
        }

        // 获取工作区详细信息
        public async Task<WorkspaceViewDto?> GetWorkspaceDetailAsync(int workspaceId, int userId)
        {
            var isMember = await _db.WorkspaceMembers
                .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);

            if (!isMember) return null;

            var workspace = await _db.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId);
            if (workspace == null) return null;

            return new WorkspaceViewDto
            {
                Id = workspace.Id,
                Name = workspace.Name,
                Type = workspace.Type,
                OwnerUserId = workspace.OwnerUserId,
                CreateTime = workspace.CreateTime,
                MemberCount = await _db.WorkspaceMembers.CountAsync(m => m.WorkspaceId == workspaceId),
                NoteCount = await _db.Notes.CountAsync(n => n.WorkspaceId == workspaceId && !n.IsDeleted)
            };
        }

        // 删除工作区
        public async Task<bool> DeleteWorkspaceAsync(int workspaceId, int userId, bool forceDelete = false)
        {
            var workspace = await _db.Workspaces
                .FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null)
                throw new BusinessException("工作区不存在。");

            // ✅ 权限检查：只有创建者或管理员可以删除
            if (workspace.OwnerUserId != userId)
                throw new BusinessException("无权删除该工作区。");

            // ✅ 检查该工作区下是否仍有笔记
            var hasNotes = await _db.Notes
                .IgnoreQueryFilters()
                .AnyAsync(n => n.WorkspaceId == workspaceId && !n.IsDeleted);

            if (hasNotes && !forceDelete)
            {
                // 普通删除（安全模式）
                throw new BusinessException("该工作区下仍有笔记，无法删除。");
            }

            if (hasNotes && forceDelete)
            {
                // 管理员彻底删除模式 → 先软删除所有笔记
                var notes = await _db.Notes
                    .IgnoreQueryFilters()
                    .Where(n => n.WorkspaceId == workspaceId)
                    .ToListAsync();

                foreach (var note in notes)
                {
                    note.IsDeleted = true;
                    note.DeletedTime = DateTime.UtcNow;
                }
                await _db.SaveChangesAsync();
            }

            // ✅ 删除成员记录（防止外键冲突）
            var members = await _db.WorkspaceMembers
                .Where(m => m.WorkspaceId == workspaceId)
                .ToListAsync();

            _db.WorkspaceMembers.RemoveRange(members);

            // ✅ 删除工作区
            _db.Workspaces.Remove(workspace);

            await _db.SaveChangesAsync();
            return true;
        }

    }
}
