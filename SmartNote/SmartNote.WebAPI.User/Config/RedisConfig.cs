namespace SmartNote.WebAPI.User.Config
{
    /// <summary>
    /// Redis 配置（对应 appsettings:Redis 节点）
    /// </summary>
    public class RedisConfig
    {
        /// <summary>
        /// 连接字符串，如 "localhost:6379"
        /// </summary>
        public string Configuration { get; set; } = string.Empty;

        /// <summary>
        /// 可选：指定使用的数据库，-1 表示使用默认。
        /// </summary>
        public int Database { get; set; } = -1;

        public bool IsValid => !string.IsNullOrWhiteSpace(Configuration);
    }
}
