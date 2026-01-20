using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _db;

        public TaskService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<TaskViewDto>> GetTasksAsync(int userId, int workspaceId, TaskItemStatus? status)
        {
            await EnsureWorkspaceAccessAsync(userId, workspaceId);

            var query = _db.TaskItems
                .Where(t => t.WorkspaceId == workspaceId)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var tasks = await query
                .OrderBy(t => t.SortOrder)
                .ThenByDescending(t => t.LastUpdateTime)
                .ToListAsync();

            return tasks.Select(MapToDto);
        }

        public async Task<IEnumerable<TaskViewDto>> GetTasksByRangeAsync(int userId, DateTime start, DateTime end)
        {
            var workspaceIds = await GetAccessibleWorkspaceIdsAsync(userId);
            var tasks = await _db.TaskItems
                .Where(t => workspaceIds.Contains(t.WorkspaceId) &&
                            (t.StartAt == null || t.StartAt <= end) &&
                            (t.DueAt == null || t.DueAt >= start))
                .OrderBy(t => t.DueAt)
                .ToListAsync();

            return tasks.Select(MapToDto);
        }

        public async Task<int> CreateTaskAsync(int userId, TaskCreateDto dto)
        {
            await EnsureWorkspaceAccessAsync(userId, dto.WorkspaceId);

            var task = new TaskItem
            {
                WorkspaceId = dto.WorkspaceId,
                NoteId = dto.NoteId,
                OwnerUserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                SortOrder = dto.SortOrder,
                StartAt = dto.StartAt,
                DueAt = dto.DueAt
            };

            _db.TaskItems.Add(task);
            await _db.SaveChangesAsync();
            return task.Id;
        }

        public async Task UpdateTaskAsync(int userId, int taskId, TaskUpdateDto dto)
        {
            var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                throw new BusinessException("任务不存在。");

            await EnsureWorkspaceAccessAsync(userId, task.WorkspaceId);

            var fromStatus = task.Status;
            var fromSortOrder = task.SortOrder;

            if (dto.NoteId.HasValue)
                task.NoteId = dto.NoteId;
            if (dto.Title != null)
                task.Title = dto.Title;
            if (dto.Description != null)
                task.Description = dto.Description;
            if (dto.Status.HasValue)
                task.Status = dto.Status.Value;
            if (dto.SortOrder.HasValue)
                task.SortOrder = dto.SortOrder.Value;
            if (dto.StartAt.HasValue)
                task.StartAt = dto.StartAt;
            if (dto.DueAt.HasValue)
                task.DueAt = dto.DueAt;

            task.LastUpdateTime = DateTime.UtcNow;

            if (fromStatus != task.Status || fromSortOrder != task.SortOrder)
            {
                _db.TaskLogs.Add(new TaskLog
                {
                    TaskItemId = task.Id,
                    ActorUserId = userId,
                    FromStatus = fromStatus,
                    ToStatus = task.Status,
                    FromSortOrder = fromSortOrder,
                    ToSortOrder = task.SortOrder
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task UpdateSortAsync(int userId, TaskSortUpdateRequest request)
        {
            if (request.Items.Count == 0)
                return;

            var taskIds = request.Items.Select(i => i.TaskId).ToList();
            var tasks = await _db.TaskItems.Where(t => taskIds.Contains(t.Id)).ToListAsync();

            foreach (var task in tasks)
            {
                await EnsureWorkspaceAccessAsync(userId, task.WorkspaceId);
                var update = request.Items.First(i => i.TaskId == task.Id);

                var fromStatus = task.Status;
                var fromSortOrder = task.SortOrder;

                task.Status = update.Status;
                task.SortOrder = update.SortOrder;
                task.LastUpdateTime = DateTime.UtcNow;

                _db.TaskLogs.Add(new TaskLog
                {
                    TaskItemId = task.Id,
                    ActorUserId = userId,
                    FromStatus = fromStatus,
                    ToStatus = task.Status,
                    FromSortOrder = fromSortOrder,
                    ToSortOrder = task.SortOrder
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int userId, int taskId)
        {
            var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                return;

            await EnsureWorkspaceAccessAsync(userId, task.WorkspaceId);

            _db.TaskItems.Remove(task);
            await _db.SaveChangesAsync();
        }

        private static TaskViewDto MapToDto(TaskItem task)
        {
            return new TaskViewDto
            {
                Id = task.Id,
                WorkspaceId = task.WorkspaceId,
                NoteId = task.NoteId,
                OwnerUserId = task.OwnerUserId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                SortOrder = task.SortOrder,
                StartAt = task.StartAt,
                DueAt = task.DueAt,
                CreateTime = task.CreateTime,
                LastUpdateTime = task.LastUpdateTime
            };
        }

        private async Task EnsureWorkspaceAccessAsync(int userId, int workspaceId)
        {
            var access = await _db.WorkspaceMembers.AnyAsync(m => m.UserId == userId && m.WorkspaceId == workspaceId)
                || await _db.Workspaces.AnyAsync(w => w.Id == workspaceId && w.OwnerUserId == userId);

            if (!access)
                throw new BusinessException("无权限访问该工作区。");
        }

        private async Task<List<int>> GetAccessibleWorkspaceIdsAsync(int userId)
        {
            return await _db.WorkspaceMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.WorkspaceId)
                .Union(
                    _db.Workspaces
                        .Where(w => w.OwnerUserId == userId)
                        .Select(w => w.Id)
                )
                .Distinct()
                .ToListAsync();
        }
    }
}
