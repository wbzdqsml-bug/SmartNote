using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Common.Helpers
{
    /// <summary>
    /// JWT 生成工具类，负责根据用户信息和配置生成访问令牌。
    /// </summary>
    public static class JwtHelper
    {
        /// <summary>
        /// 生成 JWT Token，并返回 Token 字符串与过期秒数。
        /// </summary>
        /// <param name="userId">用户 Id</param>
        /// <param name="username">用户名</param>
        /// <param name="role">角色（例如 "User"）</param>
        /// <param name="key">Jwt:Key</param>
        /// <param name="issuer">Jwt:Issuer</param>
        /// <param name="audience">Jwt:Audience</param>
        /// <param name="lifetime">令牌有效期（默认 1 小时）</param>
        public static (string token, int expiresInSeconds) GenerateToken(
            int userId,
            string username,
            string role,
            string key,
            string issuer,
            string audience,
            TimeSpan? lifetime = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("JWT key cannot be null or empty.", nameof(key));

            lifetime ??= TimeSpan.FromHours(1);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var expires = now.Add(lifetime.Value);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return (jwt, (int)lifetime.Value.TotalSeconds);
        }
    }
}
