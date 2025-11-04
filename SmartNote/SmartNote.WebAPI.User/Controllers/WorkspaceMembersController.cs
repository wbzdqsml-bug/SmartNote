using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Results;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/workspaces/{workspaceId:int}/members")]
    public class WorkspaceMembersController : ControllerBase
    {
        private readonly IWorkspaceMemberService _service;

        public WorkspaceMembersController(IWorkspaceMemberService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id))
                throw new UnauthorizedAccessException("无效的身份标识。");
            return id;
        }

        /// <summary>
        /// 获取成员列表（工作区成员可查看）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMembers([FromRoute] int workspaceId)
        {
            var userId = GetUserId();
            var list = await _service.GetMembersAsync(workspaceId, userId);
            return Ok(ApiResponse.Success(list));
        }

        public class InviteMemberRequest
        {
            [Required]
            public string Username { get; set; } = string.Empty;
        }

        /// <summary>
        /// 邀请成员（Owner 或 CanShare 成员）
        /// </summary>
        [HttpPost("invite")]
        public async Task<IActionResult> Invite([FromRoute] int workspaceId, [FromBody] InviteMemberRequest req)
        {
            var userId = GetUserId();
            await _service.InviteMemberAsync(workspaceId, userId, req.Username);
            return Ok(ApiResponse.Success("邀请成功。"));
        }

        /// <summary>
        /// 移除成员（仅 Owner）
        /// </summary>
        [HttpDelete("{targetUserId:int}")]
        public async Task<IActionResult> Remove([FromRoute] int workspaceId, [FromRoute] int targetUserId)
        {
            var userId = GetUserId();
            await _service.RemoveMemberAsync(workspaceId, userId, targetUserId);
            return Ok(ApiResponse.Success("移除成功。"));
        }

        public class UpdatePermissionsRequest
        {
            public bool CanEdit { get; set; }
            public bool CanShare { get; set; }
        }

        /// <summary>
        /// 更新成员权限（仅 Owner）
        /// </summary>
        [HttpPatch("{targetUserId:int}/permissions")]
        public async Task<IActionResult> UpdatePermissions([FromRoute] int workspaceId, [FromRoute] int targetUserId, [FromBody] UpdatePermissionsRequest req)
        {
            var userId = GetUserId();
            await _service.UpdatePermissionsAsync(workspaceId, userId, targetUserId, req.CanEdit, req.CanShare);
            return Ok(ApiResponse.Success("权限更新成功。"));
        }

        /// <summary>
        /// 退出工作区（成员）
        /// </summary>
        [HttpPost("leave")]
        public async Task<IActionResult> Leave([FromRoute] int workspaceId)
        {
            var userId = GetUserId();
            await _service.LeaveWorkspaceAsync(workspaceId, userId);
            return Ok(ApiResponse.Success("已退出工作区。"));
        }
    }
}
