// SmartNote_Shared/DTOs/User/LoginSuccessResponseDto.cs
namespace SmartNote_Shared.DTOs.User
{
    public class LoginSuccessResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;

        // VVVVVV (新增) VVVVVV
        // 用于存储用户登录后默认的个人工作区 ID
        public int PersonalWorkspaceId { get; set; }
        // ^^^^^^^^^^^^^^^^^^^^
    }
}