using SmartNote.Domain.Entities.Enums;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    /// <summary>
    /// 任务管理服务接口。
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// 获取工作区任务列表，可按状态筛选。
        /// </summary>
        Task<IEnumerable<TaskViewDto>> GetTasksAsync(int userId, int workspaceId, TaskItemStatus? status);
        /// <summary>
        /// 获取指定时间范围内的任务列表。
        /// </summary>
        Task<IEnumerable<TaskViewDto>> GetTasksByRangeAsync(int userId, DateTime start, DateTime end);
        /// <summary>
        /// 创建任务。
        /// </summary>
        Task<int> CreateTaskAsync(int userId, TaskCreateDto dto);
        /// <summary>
        /// 更新任务。
        /// </summary>
        Task UpdateTaskAsync(int userId, int taskId, TaskUpdateDto dto);
        /// <summary>
        /// 批量更新任务排序与状态。
        /// </summary>
        Task UpdateSortAsync(int userId, TaskSortUpdateRequest request);
        /// <summary>
        /// 删除任务。
        /// </summary>
        Task DeleteTaskAsync(int userId, int taskId);
    }
}
