namespace SmartNote.WebAPI.Admin.Config
{
    /// <summary>
    /// Redis 配置（与 appsettings:Redis 对应）
    /// </summary>
    public class RedisConfig
    {
        public string Configuration { get; set; } = string.Empty;
        public int Database { get; set; } = -1;

        public bool IsValid => !string.IsNullOrWhiteSpace(Configuration);
    }
}
