﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    /// <summary>
    /// 回收站管理控制器。
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecycleController : ControllerBase
    {
        private readonly IRecycleService _recycleService;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public RecycleController(IRecycleService recycleService, ApplicationDbContext db, IWebHostEnvironment env)
        {
            _recycleService = recycleService;
            _db = db;
            _env = env;
        }

        /// <summary>
        /// 获取回收站中的笔记列表。
        /// </summary>
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

        /// <summary>
        /// 恢复回收站中的笔记。
        /// </summary>
        [HttpPost("restore")]
        public async Task<IActionResult> Restore([FromBody] List<int> noteIds)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var count = await _recycleService.RestoreNotesAsync(noteIds, userId);
            return Ok(new { message = $"成功恢复 {count} 条笔记" });
        }

        /// <summary>
        /// 永久删除笔记。
        /// </summary>
        [HttpDelete("permanent")]
        public async Task<IActionResult> DeletePermanently([FromBody] List<int> noteIds)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var attachmentPaths = await _db.NoteAttachments
                .Where(a => noteIds.Contains(a.NoteId))
                .Select(a => a.StoragePath)
                .ToListAsync();
            var count = await _recycleService.PermanentlyDeleteAsync(noteIds, userId);
            RemoveAttachmentFiles(attachmentPaths);
            return Ok(new { message = $"成功永久删除 {count} 条笔记" });
        }

        /// <summary>
        /// 清理已删除笔记对应的附件物理文件。
        /// </summary>
        private void RemoveAttachmentFiles(IEnumerable<string> storagePaths)
        {
            if (storagePaths == null)
                return;

            var storageRoot = Path.Combine(_env.ContentRootPath, "storage");
            foreach (var storagePath in storagePaths.Distinct())
            {
                if (string.IsNullOrWhiteSpace(storagePath))
                    continue;

                var relativePath = storagePath.Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(storageRoot, relativePath);

                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
        }
    }
}
