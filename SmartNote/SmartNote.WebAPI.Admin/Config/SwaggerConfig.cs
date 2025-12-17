namespace SmartNote.WebAPI.Admin.Config
{
    /// <summary>
    /// Swagger/OpenAPI 配置（与 appsettings:Swagger 对应）
    /// </summary>
    public class SwaggerConfig
    {
        public string Title { get; set; } = "SmartNote 管理端 API";
        public string Version { get; set; } = "v1";
        public string? Description { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
