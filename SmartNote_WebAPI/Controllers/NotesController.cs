// SmartNote_WebAPI/Controllers/NotesController.cs
using Microsoft.AspNetCore.Authorization; // (1. 引入授权)
using Microsoft.AspNetCore.Mvc;
using SmartNote_BLL; // (2. 引入 BLL)
using SmartNote_Shared.DTOs.Note; // (3. 引入 DTOs)
using System.Security.Claims; // (4. 引入 Claims 用于获取用户ID)
using System.IdentityModel.Tokens.Jwt;

namespace SmartNote_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // (5. 关键! 保护此控制器下的所有 Action)
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;

        // 6. 注入 BLL 的 INoteService
        public NotesController(INoteService noteService)
        {
            _noteService = noteService;
        }

        /// <summary>
        /// (私有辅助方法) 从 Token 中获取当前登录用户的 ID
        /// </summary>
        private int GetCurrentUserId()
        {
            // HttpContext.User.Claims 包含了 JWT Token 解码后的所有声明 (Claims)
            // 我们在 AuthService 中存入的是 JwtRegisteredClaimNames.Sub
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier) ??
                              User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            // 如果 Token 中没有 ID (理论上不应该发生)，抛出异常
            throw new UnauthorizedAccessException("无法从 Token 中解析用户 ID。");
        }

        // --- 7. 实现 API 终结点 ---

        /// <summary>
        /// 获取当前用户的所有笔记列表（预览）
        /// GET: api/Notes
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyNotes()
        {
            try
            {
                var userId = GetCurrentUserId(); // 从 Token 获取 ID
                var notes = await _noteService.GetNotesForUserAsync(userId);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                // (未来应记录日志)
                return StatusCode(500, new { Message = $"获取笔记列表时出错: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取单篇笔记详情
        /// GET: api/Notes/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            try
            {
                var userId = GetCurrentUserId(); // 从 Token 获取 ID
                var note = await _noteService.GetNoteByIdAsync(id, userId);
                return Ok(note);
            }
            catch (KeyNotFoundException ex) // BLL 抛出：笔记未找到
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex) // BLL 抛出：无权访问
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"获取笔记详情时出错: {ex.Message}" });
            }
        }

        /// <summary>
        /// 创建新笔记
        /// POST: api/Notes
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] NoteCreateDto createDto)
        {
            // (模型验证由 [ApiController] 自动处理)
            try
            {
                var userId = GetCurrentUserId(); // 从 Token 获取 ID
                var newNote = await _noteService.CreateNoteAsync(createDto, userId);

                // 返回 201 Created，并提供新资源的访问 URI 和内容
                return CreatedAtAction(nameof(GetNoteById), new { id = newNote.Id }, newNote);
            }
            catch (UnauthorizedAccessException ex) // BLL 抛出：无权在此工作区创建
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"创建笔记时出错: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新笔记
        /// PUT: api/Notes/{id}
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] NoteUpdateDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest(new { Message = "URL 中的 ID 与请求体中的 ID 不匹配。" });
            }

            // (模型验证由 [ApiController] 自动处理)
            try
            {
                var userId = GetCurrentUserId(); // 从 Token 获取 ID
                var updatedNote = await _noteService.UpdateNoteAsync(updateDto, userId);
                return Ok(updatedNote); // 返回 200 OK 和更新后的笔记
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"更新笔记时出错: {ex.Message}" });
            }
        }

        /// <summary>
        /// 删除笔记
        /// DELETE: api/Notes/{id}
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            try
            {
                var userId = GetCurrentUserId(); // 从 Token 获取 ID
                await _noteService.DeleteNoteAsync(id, userId);
                return NoContent(); // 返回 204 No Content
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"删除笔记时出错: {ex.Message}" });
            }
        }
    }
}