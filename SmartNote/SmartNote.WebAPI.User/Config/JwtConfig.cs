using System;

namespace SmartNote.WebAPI.User.Config
{
    /// <summary>
    /// 强类型 JWT 配置，便于在 Program 中绑定与校验。
    /// </summary>
    public class JwtConfig
    {
        /// <summary>
        /// 对称加密密钥
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 发行者
        /// </summary>
        public string Issuer { get; set; } = "SmartNote.UserAPI";

        /// <summary>
        /// 受众
        /// </summary>
        public string Audience { get; set; } = "SmartNoteClient";

        /// <summary>
        /// token 生命周期（默认 1 小时）
        /// </summary>
        public TimeSpan Lifetime { get; set; } = TimeSpan.FromHours(1);

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Key))
                throw new InvalidOperationException("Jwt:Key 未配置。");
        }
    }
}
