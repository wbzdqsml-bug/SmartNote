using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var id))
                throw new UnauthorizedAccessException("无效的身份标识。");
            return id;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCategories()
        {
            var userId = GetUserId();
            var list = await _service.GetUserCategoriesAsync(userId);
            return Ok(ApiResponse.Success(list));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateRequest req)
        {
            var userId = GetUserId();
            var id = await _service.CreateCategoryAsync(userId, req);
            return Ok(ApiResponse.Success(id, "创建分类成功。"));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CategoryUpdateRequest req)
        {
            var userId = GetUserId();
            await _service.UpdateCategoryAsync(userId, id, req);
            return Ok(ApiResponse.Success("分类更新成功。"));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var userId = GetUserId();
            await _service.DeleteCategoryAsync(userId, id);
            return Ok(ApiResponse.Success("分类删除成功。"));
        }
    }
}
