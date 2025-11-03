using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;

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
            if (!ok) return BadRequest("用户名已存在");

            return Ok(new { message = "注册成功" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var res = await _auth.LoginAsync(request);
            if (res is null) return Unauthorized("用户名或密码错误");

            return Ok(res);
        }
    }
}
