using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface IAuthService
    {
        /// <summary>
        /// 注册新用户并创建个人工作区（事务保障）
        /// </summary>
        Task<bool> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// 登录并签发 JWT；把 token 写入 Redis（后续可用于黑名单/会话管理）
        /// </summary>
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}
