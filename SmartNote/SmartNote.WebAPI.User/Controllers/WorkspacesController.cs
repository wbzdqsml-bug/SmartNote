using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;
using SmartNote.Common.Extensions;

namespace SmartNote.WebAPI.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkspacesController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;

        public WorkspacesController(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        /// <summary>
        /// 创建工作区
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkspace([FromBody] WorkspaceCreateDto dto)
        {
            var userId = User.GetUserId();
            var result = await _workspaceService.CreateWorkspaceAsync(userId, dto);
            return Ok(result);
        }

        /// <summary>
        /// 获取当前用户所有工作区
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyWorkspaces()
        {
            var userId = User.GetUserId();
            var workspaces = await _workspaceService.GetUserWorkspacesAsync(userId);
            return Ok(workspaces);
        }

        /// <summary>
        /// 获取工作区详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkspaceDetail(int id)
        {
            var userId = User.GetUserId();
            var workspace = await _workspaceService.GetWorkspaceDetailAsync(id, userId);
            if (workspace == null) return Forbid("你没有访问该工作区的权限");
            return Ok(workspace);
        }

        /// <summary>
        /// 删除工作区（仅创建者）
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkspace(int id)
        {
            var userId = User.GetUserId();
            var success = await _workspaceService.DeleteWorkspaceAsync(id, userId);
            if (!success) return Forbid("无权删除该工作区");
            return Ok(new { message = "删除成功" });
        }
    }
}
