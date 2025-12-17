using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SmartNote.Common.Configs;
using SmartNote.Domain.Exceptions;

namespace SmartNote.BLL.Ai
{
    /// <summary>
    /// 极简 OpenAI Chat Completions 客户端（不依赖额外 SDK）。
    /// </summary>
    public class OpenAiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _http;
        private readonly AiOptions _options;

        public OpenAiClient(HttpClient http, AiOptions options)
        {
            _http = http;
            _options = options;
        }

        public async Task<string> ChatJsonAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                throw new BusinessException("AI 功能未启用。");

            if (!string.Equals(_options.Provider, "OpenAI", StringComparison.OrdinalIgnoreCase))
                throw new BusinessException($"当前 AI Provider 不支持：{_options.Provider}");

            var apiKey = _options.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new BusinessException("未配置 OpenAI API Key（Ai:ApiKey 或环境变量 OPENAI_API_KEY）。");

            // 优先使用 JSON mode；若模型/网关不支持，再降级到普通模式并做 JSON 截取。
            var body = await SendChatAsync(apiKey, systemPrompt, userPrompt, useJsonMode: true, cancellationToken: cancellationToken);
            if (body.IsError && body.StatusCode == 400 &&
                body.Content.Contains("response_format", StringComparison.OrdinalIgnoreCase))
            {
                body = await SendChatAsync(apiKey, systemPrompt, userPrompt, useJsonMode: false, cancellationToken: cancellationToken);
            }

            if (body.IsError)
                throw new BusinessException($"AI 请求失败（HTTP {body.StatusCode}）：{body.Content}");

            // 解析 OpenAI 响应并提取 message.content
            using var doc = JsonDocument.Parse(body.Content);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
                throw new BusinessException("AI 返回内容为空。");

            // 有些模型可能仍会包裹 ```json ... ```，这里做一次兜底清洗
            return ExtractJsonObject(content);
        }

        private async Task<ChatResult> SendChatAsync(
            string apiKey,
            string systemPrompt,
            string userPrompt,
            bool useJsonMode,
            CancellationToken cancellationToken)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            object payload = useJsonMode
                ? new
                {
                    model = _options.Model,
                    temperature = _options.Temperature,
                    response_format = new { type = "json_object" },
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    }
                }
                : new
                {
                    model = _options.Model,
                    temperature = _options.Temperature,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    }
                };

            req.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(req, cancellationToken);
            var content = await resp.Content.ReadAsStringAsync(cancellationToken);
            return resp.IsSuccessStatusCode
                ? ChatResult.Success(content)
                : ChatResult.Error((int)resp.StatusCode, content);
        }

        private readonly record struct ChatResult(bool IsError, int StatusCode, string Content)
        {
            public static ChatResult Success(string content) => new(false, 200, content);
            public static ChatResult Error(int statusCode, string content) => new(true, statusCode, content);
        }

        private static string ExtractJsonObject(string text)
        {
            var trimmed = text.Trim();
            if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
                return trimmed;

            // 去掉 ```json / ``` 包裹
            trimmed = trimmed
                .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            var start = trimmed.IndexOf('{');
            var end = trimmed.LastIndexOf('}');
            if (start >= 0 && end > start)
                return trimmed.Substring(start, end - start + 1);

            // 最差情况直接返回，让上层 JsonDocument.Parse 抛异常提示
            return trimmed;
        }
    }
}
