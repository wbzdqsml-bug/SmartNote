using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    /// <summary>
    /// 好友关系管理控制器
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/friends")]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendService _service;

        public FriendsController(IFriendService service)
        {
            _service = service;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// 获取我的好友列表
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyFriends()
        {
            var userId = GetUserId();
            var friends = await _service.GetMyFriendsAsync(userId);
            return Ok(ApiResponse.Success(friends));
        }

        /// <summary>
        /// 发送好友申请
        /// </summary>
        [HttpPost("request/{targetUsername}")]
        public async Task<IActionResult> SendRequest(string targetUsername)
        {
            var userId = GetUserId();
            await _service.SendRequestAsync(userId, targetUsername);
            return Ok(ApiResponse.Success("好友申请已发送"));
        }

        /// <summary>
        /// 获取我收到的好友申请
        /// </summary>
        [HttpGet("requests")]
        public async Task<IActionResult> GetRequests()
        {
            var userId = GetUserId();
            var list = await _service.GetRequestsAsync(userId);
            return Ok(ApiResponse.Success(list));
        }

        /// <summary>
        /// 处理好友申请（接受/拒绝）
        /// </summary>
        [HttpPost("requests/{requestId:int}/{decision}")]
        public async Task<IActionResult> HandleRequest(int requestId, string decision)
        {
            var userId = GetUserId();
            await _service.HandleRequestAsync(userId, requestId, decision);
            return Ok(ApiResponse.Success($"已{decision}好友申请"));
        }
    }
}
