﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    /// <summary>
    /// 笔记管理控制器。
    /// </summary>
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

        /// <summary>
        /// 获取当前登录用户的 ID。
        /// </summary>
        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id))
                throw new UnauthorizedAccessException("无效的身份标识。");
            return id;
        }

        /// <summary>
        /// 获取所有笔记（包含分类和标签信息）。
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetUserNotesAsync(GetUserId());
            return Ok(ApiResponse.Success(list));
        }

        /// <summary>
        /// 根据 ID 获取单条笔记详情（含分类、标签）。
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var note = await _service.GetNoteByIdAsync(GetUserId(), id);
            return note == null
                ? NotFound(ApiResponse.Fail("未找到笔记或无权限"))
                : Ok(ApiResponse.Success(note));
        }

        /// <summary>
        /// 根据分类或标签筛选笔记。
        /// </summary>
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

        /// <summary>
        /// 获取指定日期修改的笔记列表（用于热力图点击交互）。
        /// </summary>
        /// <param name="date">查询日期</param>
        [HttpGet("by-date")]
        public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
        {
            // 获取当前用户所有笔记
            var allNotes = await _service.GetUserNotesAsync(GetUserId());
            
            // 筛选出指定日期修改的笔记
            var list = allNotes.Where(n => n.LastUpdateTime.Date == date.Date)
                               .OrderByDescending(n => n.LastUpdateTime) // 按最后修改时间倒序排列
                               .ToList();
            
            return Ok(ApiResponse.Success(list));
        }

        /// <summary>
        /// 创建新笔记。
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NoteCreateDto dto)
        {
            var id = await _service.CreateNoteAsync(GetUserId(), dto);
            return Ok(ApiResponse.Success(new { id }, "创建成功"));
        }

        /// <summary>
        /// 更新笔记内容或分类。
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] NoteUpdateDto dto)
        {
            await _service.UpdateNoteAsync(id, GetUserId(), dto);
            return Ok(ApiResponse.Success("更新成功"));
        }

        /// <summary>
        /// 更新笔记的标签关联。
        /// </summary>
        [HttpPut("{id:int}/tags")]
        public async Task<IActionResult> UpdateTags(int id, [FromBody] NoteTagUpdateRequest req)
        {
            await _service.UpdateNoteTagsAsync(GetUserId(), id, req.TagIds);
            return Ok(ApiResponse.Success("标签更新成功"));
        }

        /// <summary>
        /// 软删除笔记（移动到回收站）。
        /// </summary>
        [HttpPost("soft-delete")]
        public async Task<IActionResult> SoftDelete([FromBody] List<int> ids)
        {
            var count = await _service.SoftDeleteAsync(ids, GetUserId());
            return Ok(ApiResponse.Success($"{count} 条笔记已移动到回收站"));
        }

        /// <summary>
        /// 导入文件生成笔记 (支持 .md, .json, .txt, .html)
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] IFormFile file, [FromQuery] int workspaceId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.Fail("请上传有效文件"));

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(ApiResponse.Fail("文件大小不能超过 10MB"));

            try
            {
                using var stream = file.OpenReadStream();
                var id = await _service.ImportNoteAsync(GetUserId(), workspaceId, file.FileName, stream);
                return Ok(ApiResponse.Success(new { id }, "导入成功"));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
