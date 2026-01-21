using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    /// <summary>
    /// 任务看板与日程相关接口。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _service;

        /// <summary>
        /// 初始化任务控制器。
        /// </summary>
        public TasksController(ITaskService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取当前登录用户 Id。
        /// </summary>
        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id))
                throw new UnauthorizedAccessException("无效的身份标识。");
            return id;
        }

        /// <summary>
        /// 获取工作区任务列表，可按状态过滤。
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetByWorkspace([FromQuery] int workspaceId, [FromQuery] TaskItemStatus? status)
        {
            try
            {
                var data = await _service.GetTasksAsync(GetUserId(), workspaceId, status);
                return Ok(ApiResponse.Success(data));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取指定时间范围内的任务列表。
        /// </summary>
        [HttpGet("range")]
        public async Task<IActionResult> GetByRange([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            try
            {
                var data = await _service.GetTasksByRangeAsync(GetUserId(), start, end);
                return Ok(ApiResponse.Success(data));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 创建任务。
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskCreateDto dto)
        {
            try
            {
                var id = await _service.CreateTaskAsync(GetUserId(), dto);
                return Ok(ApiResponse.Success(new { id }, "创建成功"));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 更新任务。
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskUpdateDto dto)
        {
            try
            {
                await _service.UpdateTaskAsync(GetUserId(), id, dto);
                return Ok(ApiResponse.Success("更新成功"));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 批量更新任务排序和状态。
        /// </summary>
        [HttpPut("sort")]
        public async Task<IActionResult> UpdateSort([FromBody] TaskSortUpdateRequest request)
        {
            try
            {
                await _service.UpdateSortAsync(GetUserId(), request);
                return Ok(ApiResponse.Success("排序更新成功"));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 删除任务。
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteTaskAsync(GetUserId(), id);
                return Ok(ApiResponse.Success("删除成功"));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
