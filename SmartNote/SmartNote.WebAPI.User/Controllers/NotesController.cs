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
    [Route("api/notes")]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _service;

        public NotesController(INoteService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id))
                throw new UnauthorizedAccessException("无效的身份标识。");
            return id;
        }

        // ================================  
        // 获取所有笔记（完整：分类 + 标签）
        // ================================
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetUserNotesAsync(GetUserId());
            return Ok(ApiResponse.Success(list));
        }

        // ================================  
        // 获取单条笔记（含分类、标签）
        // ================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var note = await _service.GetNoteByIdAsync(GetUserId(), id);
            return note == null
                ? NotFound(ApiResponse.Fail("未找到笔记或无权限"))
                : Ok(ApiResponse.Success(note));
        }

        // ================================  
        // 筛选（分类 + 标签）
        // ================================
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(
            [FromQuery] int? categoryId,
            [FromQuery] string? tagIds)
        {
            List<int>? tagIdList = null;
            if (!string.IsNullOrWhiteSpace(tagIds))
                tagIdList = tagIds.Split(',').Select(int.Parse).ToList();

            var list = await _service.FilterNotesAsync(GetUserId(), categoryId, tagIdList);
            return Ok(ApiResponse.Success(list));
        }

        // ================================  
        // 创建
        // ================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NoteCreateDto dto)
        {
            var id = await _service.CreateNoteAsync(GetUserId(), dto);
            return Ok(ApiResponse.Success(new { id }, "创建成功"));
        }

        // ================================  
        // 更新内容 / 分类
        // ================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] NoteUpdateDto dto)
        {
            await _service.UpdateNoteAsync(id, GetUserId(), dto);
            return Ok(ApiResponse.Success("更新成功"));
        }

        // ================================  
        // 更新标签（多对多）
        // ================================
        [HttpPut("{id:int}/tags")]
        public async Task<IActionResult> UpdateTags(int id, [FromBody] NoteTagUpdateRequest req)
        {
            await _service.UpdateNoteTagsAsync(GetUserId(), id, req.TagIds);
            return Ok(ApiResponse.Success("标签更新成功"));
        }

        // ================================  
        // 软删除
        // ================================
        [HttpPost("soft-delete")]
        public async Task<IActionResult> SoftDelete([FromBody] List<int> ids)
        {
            var count = await _service.SoftDeleteAsync(ids, GetUserId());
            return Ok(ApiResponse.Success($"{count} 条笔记已移动到回收站"));
        }
    }
}
