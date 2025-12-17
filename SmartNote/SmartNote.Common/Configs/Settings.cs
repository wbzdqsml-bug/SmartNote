namespace SmartNote.Common.Configs
{
    /// <summary>
    /// 提供配置节名称常量，避免魔法字符串散落。
    /// </summary>
    public static class Settings
    {
        public const string JwtSection = "Jwt";
        public const string RedisSection = "Redis";
        public const string CorsSection = "Cors";
        public const string SwaggerSection = "Swagger";
    }
}
