using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    /// <summary>
    /// 聊天历史记录控制器
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _service;

        public ChatController(IChatService service)
        {
            _service = service;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// 获取私聊历史记录
        /// </summary>
        [HttpGet("private/{friendId:int}")]
        public async Task<IActionResult> GetPrivateHistory(int friendId)
        {
            var userId = GetUserId();
            var messages = await _service.GetPrivateHistoryAsync(userId, friendId);
            return Ok(ApiResponse.Success(messages));
        }

        /// <summary>
        /// 获取工作区群聊历史记录
        /// </summary>
        [HttpGet("workspace/{workspaceId:int}")]
        public async Task<IActionResult> GetWorkspaceHistory(int workspaceId)
        {
            var userId = GetUserId();
            var messages = await _service.GetWorkspaceHistoryAsync(userId, workspaceId);
            return Ok(ApiResponse.Success(messages));
        }
    }
}