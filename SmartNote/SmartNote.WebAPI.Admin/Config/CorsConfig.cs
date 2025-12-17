namespace SmartNote.WebAPI.Admin.Config
{
    /// <summary>
    /// CORS 配置（与 appsettings:Cors 对应）
    /// </summary>
    public class CorsConfig
    {
        public string[] Origins { get; set; } = System.Array.Empty<string>();
        public bool AllowCredentials { get; set; } = true;
    }
}
