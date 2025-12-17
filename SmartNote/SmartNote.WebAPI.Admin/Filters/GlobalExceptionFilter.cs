using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Results;
using System.Net;

namespace SmartNote.WebAPI.Admin.Filters
{
    /// <summary>
    /// 全局异常过滤器：统一异常输出，避免散落 try/catch。
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            ApiResponse body;
            var statusCode = (int)HttpStatusCode.InternalServerError;

            switch (ex)
            {
                case BusinessException bex:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    body = ApiResponse.Fail(bex.Message, 4001);
                    break;
                case PermissionDeniedException pex:
                    statusCode = (int)HttpStatusCode.Forbidden;
                    body = ApiResponse.Fail(pex.Message, 4030);
                    break;
                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    body = ApiResponse.Fail("未授权访问，请重新登录。", 4010);
                    break;
                case KeyNotFoundException kex:
                    statusCode = (int)HttpStatusCode.NotFound;
                    body = ApiResponse.Fail(kex.Message, 4040);
                    break;
                default:
                    body = ApiResponse.Fail("服务器发生内部错误，请稍后再试。", 5000);
                    break;
            }

            context.Result = new ObjectResult(body) { StatusCode = statusCode };
            context.ExceptionHandled = true;
        }
    }
}
