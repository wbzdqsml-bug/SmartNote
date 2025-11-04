using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Results;
using System.Net;

namespace SmartNote.WebAPI.User.Filters
{
    /// <summary>
    /// 全局异常过滤器，用于统一处理控制器中的未捕获异常。
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
            var exception = context.Exception;
            ApiResponse response;
            int statusCode;

            switch (exception)
            {
                // 自定义业务异常
                case BusinessException bex:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse.Fail(bex.Message, code: 4001);
                    _logger.LogWarning("[业务异常] {Message}", bex.Message);
                    break;

                // 未授权异常
                case UnauthorizedAccessException uex:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    response = ApiResponse.Fail("未授权访问，请重新登录。", code: 4010);
                    _logger.LogWarning("[未授权访问] {Message}", uex.Message);
                    break;

                // 资源不存在异常（可选）
                case KeyNotFoundException kex:
                    statusCode = (int)HttpStatusCode.NotFound;
                    response = ApiResponse.Fail(kex.Message, code: 4040);
                    _logger.LogWarning("[资源未找到] {Message}", kex.Message);
                    break;

                // 其他系统异常
                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    response = ApiResponse.Fail("服务器发生内部错误，请稍后再试。", code: 5000);
                    _logger.LogError(exception, "[系统异常] {Message}", exception.Message);
                    break;
            }

            context.Result = new ObjectResult(response)
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
        }
    }
}
