using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;
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

        /// <summary>
        /// 获取当前用户的所有笔记（自动过滤软删除）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notes = await _noteService.GetUserNotesAsync(userId);
            return Ok(notes);
        }

        /// <summary>
        /// 创建新笔记
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] NoteCreateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("笔记标题不能为空");

            var id = await _noteService.CreateNoteAsync(userId, dto);
            return Ok(new { message = "创建成功", noteId = id });
        }

        /// <summary>
        /// 更新笔记内容
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] NoteUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (dto == null)
                return BadRequest("请求数据无效");

            var result = await _noteService.UpdateNoteAsync(id, userId, dto);
            if (result > 0)
                return Ok(new { message = "更新成功" });

            return NotFound("笔记不存在或无权访问");
        }

        /// <summary>
        /// 批量软删除笔记（移动到回收站）
        /// </summary>
        [HttpPost("soft")]
        public async Task<IActionResult> SoftDeleteNotes([FromBody] List<int> noteIds)
        {
            if (noteIds == null || !noteIds.Any())
                return BadRequest("请选择要删除的笔记");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var count = await _noteService.SoftDeleteAsync(noteIds, userId);
            return Ok(new { message = $"已成功删除 {count} 条笔记（已移动到回收站）" });
        }
    }
}
