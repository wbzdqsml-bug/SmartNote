﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    /// <summary>
    /// 用户个人资料管理控制器。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/user/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _service;

        public ProfileController(IProfileService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取当前登录用户的 ID。
        /// </summary>
        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var id))
                throw new UnauthorizedAccessException("无效身份标识");
            return id;
        }

        /// <summary>
        /// 获取当前用户的个人资料。
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var data = await _service.GetProfileAsync(userId);
            return Ok(ApiResponse.Success(data));
        }

        /// <summary>
        /// 更新当前用户的个人资料。
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest req)
        {
            var userId = GetUserId();
            await _service.UpdateProfileAsync(userId, req);
            return Ok(ApiResponse.Success("资料已更新"));
        }
    }
}
