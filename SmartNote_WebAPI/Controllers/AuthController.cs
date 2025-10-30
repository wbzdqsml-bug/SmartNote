// SmartNote_WebAPI/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using SmartNote_BLL; // 1. 引入 BLL 命名空间
using SmartNote_Shared.DTOs.User; // 2. 引入 Shared DTOs 命名空间
using System;
using System.Threading.Tasks;

namespace SmartNote_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // 路由将是 /api/Auth
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService; // 3. 声明 BLL 服务接口

        // 4. 通过构造函数注入 BLL 服务
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        [HttpPost("register")] // 终结点: POST /api/Auth/register
        public async Task<IActionResult> Register([FromBody] UserRegisterRequestDto request)
        {
            // (模型验证由 [ApiController] 自动处理)
            try
            {
                var message = await _authService.RegisterAsync(request);
                return Ok(new { Message = message }); // 返回 200 OK
            }
            catch (InvalidOperationException ex) // BLL 抛出 "用户名已存在" 异常
            {
                return BadRequest(new { Message = ex.Message }); // 返回 400 Bad Request
            }
            catch (Exception ex) // BLL 抛出其他数据库异常
            {
                // (应该记录日志)
                return StatusCode(500, new { Message = "注册时发生内部错误。", Detail = ex.Message });
            }
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        [HttpPost("login")] // 终结点: POST /api/Auth/login
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request)
        {
            // (模型验证由 [ApiController] 自动处理)
            try
            {
                var responseDto = await _authService.LoginAsync(request);
                return Ok(responseDto); // 返回 200 OK，并携带 Token 和用户信息
            }
            catch (UnauthorizedAccessException ex) // BLL 抛出 "用户名或密码错误" 异常
            {
                return Unauthorized(new { Message = ex.Message }); // 返回 410 Unauthorized
            }
            catch (Exception ex) // BLL 抛出其他异常
            {
                // (应该记录日志)
                return StatusCode(500, new { Message = "登录时发生内部错误。", Detail = ex.Message });
            }
        }
    }
}