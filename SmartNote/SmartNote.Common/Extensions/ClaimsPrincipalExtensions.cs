using System.Security.Claims;

namespace SmartNote.Common.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// 从 JWT Claims 中提取用户 ID
        /// </summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new UnauthorizedAccessException("无法从令牌中获取用户ID");

            if (int.TryParse(claim.Value, out var userId))
                return userId;

            throw new UnauthorizedAccessException("无效的用户ID格式");
        }

        /// <summary>
        /// 获取用户名（如果存在）
        /// </summary>
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value ?? "UnknownUser";
        }

        /// <summary>
        /// 获取角色名称（可选）
        /// </summary>
        public static string? GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
