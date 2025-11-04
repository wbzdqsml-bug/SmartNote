using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api")]
    public class WorkspaceInvitationsController : ControllerBase
    {
        private readonly IWorkspaceInvitationService _service;

        public WorkspaceInvitationsController(IWorkspaceInvitationService service)
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
        /// 我收到的邀请列表
        /// </summary>
        [HttpGet("invitations")]
        public async Task<IActionResult> MyInvitations()
        {
            var userId = GetUserId();
            var list = await _service.GetUserInvitationsAsync(userId);
            return Ok(ApiResponse.Success(list));
        }

        /// <summary>
        /// 在工作区发送邀请（Owner 或具有分享权限的成员）
        /// </summary>
        [HttpPost("workspaces/{workspaceId:int}/invitations")]
        public async Task<IActionResult> Send([FromRoute] int workspaceId, [FromBody] WorkspaceInvitationSendDto dto)
        {
            var userId = GetUserId();
            await _service.SendInvitationAsync(workspaceId, userId, dto);
            return Ok(ApiResponse.Success("邀请已发送。"));
        }

        /// <summary>
        /// 接受邀请（被邀请人）
        /// </summary>
        [HttpPost("invitations/{invitationId:int}/accept")]
        public async Task<IActionResult> Accept([FromRoute] int invitationId)
        {
            var userId = GetUserId();
            await _service.AcceptInvitationAsync(invitationId, userId);
            return Ok(ApiResponse.Success("已接受邀请并加入工作区。"));
        }

        /// <summary>
        /// 拒绝邀请（被邀请人）
        /// </summary>
        [HttpPost("invitations/{invitationId:int}/reject")]
        public async Task<IActionResult> Reject([FromRoute] int invitationId)
        {
            var userId = GetUserId();
            await _service.RejectInvitationAsync(invitationId, userId);
            return Ok(ApiResponse.Success("已拒绝邀请。"));
        }

        /// <summary>
        /// 撤销邀请（发起人或工作区拥有者）
        /// </summary>
        [HttpDelete("invitations/{invitationId:int}")]
        public async Task<IActionResult> Revoke([FromRoute] int invitationId)
        {
            var userId = GetUserId();
            await _service.RevokeInvitationAsync(invitationId, userId);
            return Ok(ApiResponse.Success("邀请已撤销。"));
        }
    }
}
