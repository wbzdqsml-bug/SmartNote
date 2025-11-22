using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tags")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _service;

        public TagsController(ITagService service)
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

        /// <summary>
        /// 当前用户的所有标签列表
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyTags()
        {
            var userId = GetUserId();
            var list = await _service.GetUserTagsAsync(userId);
            return Ok(ApiResponse.Success(list));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TagCreateRequest req)
        {
            var userId = GetUserId();
            var id = await _service.CreateTagAsync(userId, req);
            return Ok(ApiResponse.Success(id, "创建标签成功。"));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TagUpdateRequest req)
        {
            var userId = GetUserId();
            await _service.UpdateTagAsync(userId, id, req);
            return Ok(ApiResponse.Success("标签更新成功。"));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var userId = GetUserId();
            await _service.DeleteTagAsync(userId, id);
            return Ok(ApiResponse.Success("标签删除成功。"));
        }
    }
}
