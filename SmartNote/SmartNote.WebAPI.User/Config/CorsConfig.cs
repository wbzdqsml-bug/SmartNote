namespace SmartNote.WebAPI.User.Config
{
    /// <summary>
    /// CORS 配置（与 appsettings:Cors 对应）
    /// </summary>
    public class CorsConfig
    {
        /// <summary>
        /// 允许的 Origin 列表
        /// </summary>
        public string[] Origins { get; set; } = System.Array.Empty<string>();

        /// <summary>
        /// 是否允许携带凭证（Cookie / Auth 头）
        /// </summary>
        public bool AllowCredentials { get; set; } = true;
    }
}
