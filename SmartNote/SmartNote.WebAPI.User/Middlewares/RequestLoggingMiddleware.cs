using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmartNote.WebAPI.User.Middlewares
{
    /// <summary>
    /// 请求日志中间件：记录每个 HTTP 请求的路径、方法、耗时和结果。
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var requestInfo = new StringBuilder();

            requestInfo.AppendLine("=== 🌐 Incoming Request ===");
            requestInfo.AppendLine($"➡️ Path: {request.Method} {request.Path}");
            requestInfo.AppendLine($"🔹 Query: {request.QueryString}");
            requestInfo.AppendLine($"🔹 IP: {context.Connection.RemoteIpAddress}");
            requestInfo.AppendLine($"🔹 Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

            _logger.LogInformation(requestInfo.ToString());

            try
            {
                // 继续执行下一个中间件
                await _next(context);

                stopwatch.Stop();
                var responseStatus = context.Response.StatusCode;

                _logger.LogInformation(
                    "✅ Completed {Method} {Path} with {StatusCode} in {Elapsed} ms",
                    request.Method,
                    request.Path,
                    responseStatus,
                    stopwatch.ElapsedMilliseconds
                );
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error processing {Method} {Path} after {Elapsed} ms",
                    request.Method, request.Path, stopwatch.ElapsedMilliseconds);

                throw; // 保留异常交由 GlobalExceptionFilter 处理
            }
        }
    }

    /// <summary>
    /// 中间件扩展方法
    /// </summary>
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
