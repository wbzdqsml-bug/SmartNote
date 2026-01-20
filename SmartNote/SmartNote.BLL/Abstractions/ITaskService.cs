using SmartNote.Domain.Entities.Enums;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskViewDto>> GetTasksAsync(int userId, int workspaceId, TaskItemStatus? status);
        Task<IEnumerable<TaskViewDto>> GetTasksByRangeAsync(int userId, DateTime start, DateTime end);
        Task<int> CreateTaskAsync(int userId, TaskCreateDto dto);
        Task UpdateTaskAsync(int userId, int taskId, TaskUpdateDto dto);
        Task UpdateSortAsync(int userId, TaskSortUpdateRequest request);
        Task DeleteTaskAsync(int userId, int taskId);
    }
}
