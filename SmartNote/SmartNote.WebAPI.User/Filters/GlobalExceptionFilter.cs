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
                // 业务异常（400）
                case BusinessException bex:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse.Fail(bex.Message, code: 4001);
                    break;

                // ⭐ 权限不足（403）
                case PermissionDeniedException pex:
                    statusCode = (int)HttpStatusCode.Forbidden;
                    response = ApiResponse.Fail(pex.Message, code: 4030);
                    break;

                // 登录失效/凭证无效（401）
                case UnauthorizedAccessException uex:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    response = ApiResponse.Fail("未授权访问，请重新登录。", code: 4010);
                    break;

                // 资源未找到（404）
                case KeyNotFoundException kex:
                    statusCode = (int)HttpStatusCode.NotFound;
                    response = ApiResponse.Fail(kex.Message, code: 4040);
                    break;

                // 其他系统异常（500）
                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    response = ApiResponse.Fail("服务器发生内部错误，请稍后再试。", code: 5000);
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
