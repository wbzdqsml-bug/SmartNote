using System;

namespace SmartNote.WebAPI.Admin.Config
{
    /// <summary>
    /// 强类型 JWT 配置（与 appsettings:Jwt 对应）
    /// </summary>
    public class JwtConfig
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = "SmartNote.AdminAPI";
        public string Audience { get; set; } = "SmartNoteAdminClient";
        public TimeSpan Lifetime { get; set; } = TimeSpan.FromHours(1);

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Key))
                throw new InvalidOperationException("Jwt:Key 未配置。");
        }
    }
}
