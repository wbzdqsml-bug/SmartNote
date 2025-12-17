namespace SmartNote.WebAPI.User.Config
{
    /// <summary>
    /// Swagger/OpenAPI 配置
    /// </summary>
    public class SwaggerConfig
    {
        public string Title { get; set; } = "SmartNote 用户 API";
        public string Version { get; set; } = "v1";
        public string? Description { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
