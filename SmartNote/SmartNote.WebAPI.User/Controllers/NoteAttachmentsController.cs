using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Common.Configs;
using SmartNote.Shared.Results;
using System;
using System.IO;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notes")]
    public class NoteAttachmentsController : ControllerBase
    {
        private readonly INoteAttachmentService _service;
        private readonly IWebHostEnvironment _env;
        private readonly StorageOptions _storageOptions;

        public NoteAttachmentsController(INoteAttachmentService service, IWebHostEnvironment env, StorageOptions storageOptions)
        {
            _service = service;
            _env = env;
            _storageOptions = storageOptions;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("{noteId:int}/attachments")]
        public async Task<IActionResult> UploadAttachment(int noteId, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.Fail("附件不能为空"));

            if (_storageOptions.MaxAttachmentSizeBytes > 0 && file.Length > _storageOptions.MaxAttachmentSizeBytes)
                return BadRequest(ApiResponse.Fail("附件大小超出限制"));

            var userId = GetUserId();
            await _service.EnsureCanEditNoteAsync(userId, noteId);

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext))
                ext = ".bin";

            var storageRoot = Path.Combine(_env.ContentRootPath, "storage");
            var noteDir = Path.Combine(storageRoot, "attachments", noteId.ToString());
            Directory.CreateDirectory(noteDir);

            var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
            var fullPath = Path.Combine(noteDir, fileName);
            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("attachments", noteId.ToString(), fileName)
                .Replace('\\', '/');

            try
            {
                var dto = await _service.CreateAsync(userId, noteId, file.FileName, file.ContentType, file.Length, relativePath);
                return Ok(ApiResponse.Success(dto));
            }
            catch
            {
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
                throw;
            }
        }

        [HttpGet("{noteId:int}/attachments")]
        public async Task<IActionResult> ListAttachments(int noteId)
        {
            var userId = GetUserId();
            var list = await _service.GetByNoteAsync(userId, noteId);
            return Ok(ApiResponse.Success(list));
        }

        [HttpGet("attachments/{attachmentId:int}")]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var userId = GetUserId();
            var attachment = await _service.GetForDownloadAsync(userId, attachmentId);

            var storageRoot = Path.Combine(_env.ContentRootPath, "storage");
            var relativePath = attachment.StoragePath.Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(storageRoot, relativePath);

            if (!System.IO.File.Exists(fullPath))
                return NotFound(ApiResponse.Fail("文件不存在"));

            if (attachment.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return PhysicalFile(fullPath, attachment.ContentType, enableRangeProcessing: true);

            return PhysicalFile(fullPath, attachment.ContentType, attachment.OriginalFileName, enableRangeProcessing: true);
        }

        [HttpDelete("attachments/{attachmentId:int}")]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var userId = GetUserId();
            var attachment = await _service.DeleteAsync(userId, attachmentId);

            var storageRoot = Path.Combine(_env.ContentRootPath, "storage");
            var relativePath = attachment.StoragePath.Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(storageRoot, relativePath);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            return Ok(ApiResponse.Success("附件已删除"));
        }
    }
}
