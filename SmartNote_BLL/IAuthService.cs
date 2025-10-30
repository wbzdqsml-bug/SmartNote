// SmartNote_BLL/IAuthService.cs

// 确保命名空间与你的项目名一致
using SmartNote.Shared.DTOs.User;
using SmartNote_Shared.DTOs.User; // (确保 using SmartNote_Shared)
using System.Threading.Tasks;

namespace SmartNote_BLL // (确保这是你的 BLL 命名空间)
{
    public interface IAuthService
    {
        /// <summary>
        /// 注册新用户，并自动创建个人工作区
        /// </summary>
        /// <param name="request">注册 DTO</param>
        /// <returns>返回成功/失败的消息</returns>
        Task<string> RegisterAsync(UserRegisterRequestDto request);

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request">登录 DTO</param>
        /// <returns>登录成功则返回 LoginSuccessResponseDto，失败则抛出异常</returns>
        Task<LoginSuccessResponseDto> LoginAsync(UserLoginRequestDto request);
    }
}