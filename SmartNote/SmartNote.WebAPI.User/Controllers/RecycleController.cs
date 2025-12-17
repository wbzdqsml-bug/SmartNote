using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecycleController : ControllerBase
    {
        private readonly IRecycleService _recycleService;

        public RecycleController(IRecycleService recycleService)
        {
            _recycleService = recycleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDeletedNotes()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notes = await _recycleService.GetDeletedNotesAsync(userId);
            return Ok(notes);
        }

        /// <summary>
        /// 获取回收站中单条笔记详情（只读）
        /// </summary>
        [HttpGet("{noteId:int}")]
        public async Task<IActionResult> GetDeletedNoteDetail([FromRoute] int noteId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var note = await _recycleService.GetDeletedNoteByIdAsync(userId, noteId);
            return note == null ? NotFound(new { message = "未找到笔记或无权限" }) : Ok(note);
        }

        [HttpPost("restore")]
        public async Task<IActionResult> Restore([FromBody] List<int> noteIds)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var count = await _recycleService.RestoreNotesAsync(noteIds, userId);
            return Ok(new { message = $"成功恢复 {count} 条笔记" });
        }

        [HttpDelete("permanent")]
        public async Task<IActionResult> DeletePermanently([FromBody] List<int> noteIds)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var count = await _recycleService.PermanentlyDeleteAsync(noteIds, userId);
            return Ok(new { message = $"成功永久删除 {count} 条笔记" });
        }
    }
}
