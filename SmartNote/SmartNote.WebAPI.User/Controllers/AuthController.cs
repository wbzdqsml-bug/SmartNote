using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var ok = await _auth.RegisterAsync(request);
            if (!ok)
                return BadRequest(new { code = "USERNAME_EXISTS", message = "用户名已存在" });

            return Ok(new { message = "注册成功" });
        }

[Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { code = "INVALID_TOKEN", message = "无法获取用户身份" });

        var cache = HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        var cacheKey = $"token:{userId}";

        await cache.RemoveAsync(cacheKey);

        return Ok(new { code = "LOGOUT_SUCCESS", message = "退出成功" });
    }

    [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var res = await _auth.LoginAsync(request);
            if (res is null)
                return Unauthorized(new { code = "LOGIN_FAILED", message = "用户名或密码错误" });

            return Ok(new { code = "SUCCESS", data = res });
        }
    }
}
