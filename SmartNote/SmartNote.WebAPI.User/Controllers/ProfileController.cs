﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;
using SmartNote.Common.Configs;
using SmartNote.Shared.Results;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.IO;

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
        private readonly IWebHostEnvironment _env;
        private readonly StorageOptions _storageOptions;

        public ProfileController(IProfileService service, IWebHostEnvironment env, StorageOptions storageOptions)
        {
            _service = service;
            _env = env;
            _storageOptions = storageOptions;
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

        /// <summary>
        /// 上传头像（公开可访问）
        /// </summary>
        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.Fail("头像文件不能为空"));

            if (_storageOptions.MaxAvatarSizeBytes > 0 && file.Length > _storageOptions.MaxAvatarSizeBytes)
                return BadRequest(ApiResponse.Fail("头像大小超出限制"));

            var contentType = file.ContentType ?? string.Empty;
            if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.Fail("仅支持图片格式头像"));

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext))
                ext = ".jpg";

            var extLower = ext.ToLowerInvariant();
            var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowedExt.Contains(extLower))
                return BadRequest(ApiResponse.Fail("头像格式不支持"));

            var userId = GetUserId();
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var userDir = Path.Combine(webRoot, "avatars", userId.ToString());
            Directory.CreateDirectory(userDir);

            var fileName = $"{Guid.NewGuid():N}{extLower}";
            var fullPath = Path.Combine(userDir, fileName);
            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var oldAvatarUrl = (await _service.GetProfileAsync(userId)).AvatarUrl;
            var avatarUrl = $"/avatars/{userId}/{fileName}";

            try
            {
                await _service.UpdateAvatarAsync(userId, avatarUrl);
            }
            catch
            {
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
                throw;
            }

            DeleteOldAvatarIfLocal(webRoot, oldAvatarUrl);

            return Ok(ApiResponse.Success(new { avatarUrl }));
        }

        private static void DeleteOldAvatarIfLocal(string webRoot, string? avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(avatarUrl))
                return;

            if (!avatarUrl.StartsWith("/avatars/", StringComparison.OrdinalIgnoreCase))
                return;

            var relativePath = avatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            if (Path.IsPathRooted(relativePath))
                return;

            var avatarRoot = Path.Combine(webRoot, "avatars");
            var rootFullPath = Path.GetFullPath(avatarRoot) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(Path.Combine(webRoot, relativePath));

            if (!fullPath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
                return;

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}
