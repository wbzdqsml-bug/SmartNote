using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NotesController(INoteService noteService)
        {
            _noteService = noteService;
        }

        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id))
                throw new UnauthorizedAccessException("无效的身份标识。");
            return id;
        }

        /// <summary>
        /// 获取当前用户可访问的所有笔记（自动过滤已删除）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            var userId = GetUserId();
            var notes = await _noteService.GetUserNotesAsync(userId);
            return Ok(ApiResponse.Success(notes));
        }

        /// <summary>
        /// 根据 ID 获取笔记详情（包括内容与类型）
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            var userId = GetUserId();
            var notes = await _noteService.GetUserNotesAsync(userId);
            var note = notes.FirstOrDefault(n => n.Id == id);
            if (note == null)
                return NotFound(ApiResponse.Fail("未找到笔记或无权访问"));

            return Ok(ApiResponse.Success(note));
        }

        /// <summary>
        /// 创建新笔记
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] NoteCreateDto dto)
        {
            var userId = GetUserId();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(ApiResponse.Fail("笔记标题不能为空"));

            if (!Enum.IsDefined(typeof(NoteType), dto.Type))
                return BadRequest(ApiResponse.Fail("无效的笔记类型"));

            var id = await _noteService.CreateNoteAsync(userId, dto);
            return Ok(ApiResponse.Success(new { noteId = id }, "创建成功"));
        }

        /// <summary>
        /// 更新笔记内容
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] NoteUpdateDto dto)
        {
            var userId = GetUserId();

            if (dto == null)
                return BadRequest(ApiResponse.Fail("请求数据无效"));

            var result = await _noteService.UpdateNoteAsync(id, userId, dto);
            return result > 0
                ? Ok(ApiResponse.Success("更新成功"))
                : NotFound(ApiResponse.Fail("笔记不存在或无权访问"));
        }

        /// <summary>
        /// 批量软删除笔记（移动到回收站）
        /// </summary>
        [HttpPost("soft")]
        public async Task<IActionResult> SoftDeleteNotes([FromBody] List<int> noteIds)
        {
            if (noteIds == null || !noteIds.Any())
                return BadRequest(ApiResponse.Fail("请选择要删除的笔记"));

            var userId = GetUserId();
            var count = await _noteService.SoftDeleteAsync(noteIds, userId);
            return Ok(ApiResponse.Success(new { deleted = count }, $"已成功删除 {count} 条笔记（已移动到回收站）"));
        }
    }
}
