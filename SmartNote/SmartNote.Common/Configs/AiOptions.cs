namespace SmartNote.Common.Configs
{
    /// <summary>
    /// AI 相关配置（建议通过 appsettings 或环境变量注入）
    /// </summary>
    public class AiOptions
    {
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// 目前内置实现：OpenAI（如需其它厂商，可后续扩展）
        /// </summary>
        public string Provider { get; set; } = "OpenAI";

        /// <summary>
        /// OpenAI API Key（也可通过环境变量 OPENAI_API_KEY 提供）
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// OpenAI BaseUrl（默认 https://api.openai.com/v1）
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";

        /// <summary>
        /// 默认模型，如 gpt-4o-mini
        /// </summary>
        public string Model { get; set; } = "gpt-4o-mini";

        /// <summary>
        /// 生成温度（0 越稳定）
        /// </summary>
        public double Temperature { get; set; } = 0.2;

        /// <summary>
        /// 请求超时（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 60;
    }
}
