using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartNote.Shared.Results;

namespace SmartNote.WebAPI.Admin.Filters
{
    /// <summary>
    /// 模型验证过滤器：统一处理 ModelState 失败。
    /// </summary>
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
                return;

            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => new
                {
                    Field = e.Key,
                    Error = e.Value?.Errors.FirstOrDefault()?.ErrorMessage
                })
                .ToList();

            var message = string.Join(" | ", errors.Select(e => $"{e.Field}: {e.Error}"));
            context.Result = new BadRequestObjectResult(ApiResponse.Fail(message, 4000));
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
