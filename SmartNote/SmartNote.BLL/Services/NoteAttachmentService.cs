using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace SmartNote.BLL.Services
{
    public class NoteAttachmentService : INoteAttachmentService
    {
        private readonly ApplicationDbContext _db;
        private static readonly Regex MarkdownImageRegex = new Regex(@"!\[[^\]]*\]\(([^)]+)\)", RegexOptions.Compiled);
        private static readonly Regex MarkdownLinkRegex = new Regex(@"\[[^\]]*\]\(([^)]+)\)", RegexOptions.Compiled);
        private static readonly Regex HtmlImageRegex = new Regex("<img[^>]+src=[\"']([^\"']+)[\"'][^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex HtmlLinkRegex = new Regex("<a[^>]+href=[\"']([^\"']+)[\"'][^>]*>.*?</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public NoteAttachmentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<NoteAttachmentDto> CreateAsync(int userId, int noteId, string originalFileName, string contentType, long size, string storagePath)
        {
            await EnsureCanEditNoteAsync(userId, noteId);

            var entity = new NoteAttachment
            {
                NoteId = noteId,
                UploaderUserId = userId,
                OriginalFileName = originalFileName,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                Size = size,
                StoragePath = storagePath,
                CreatedTime = DateTime.UtcNow
            };

            _db.NoteAttachments.Add(entity);
            await _db.SaveChangesAsync();

            return Map(entity);
        }

        public async Task EnsureCanAccessNoteAsync(int userId, int noteId)
        {
            var canAccess = await CanAccessNoteAsync(userId, noteId);
            if (!canAccess)
                throw new KeyNotFoundException("笔记不存在或无权限访问。");
        }

        public async Task EnsureCanEditNoteAsync(int userId, int noteId)
        {
            await GetNoteForEditAsync(userId, noteId);
        }

        public async Task<IReadOnlyList<NoteAttachmentDto>> GetByNoteAsync(int userId, int noteId)
        {
            var canAccess = await CanAccessNoteAsync(userId, noteId);
            if (!canAccess)
                throw new KeyNotFoundException("笔记不存在或无权限访问。");

            var list = await _db.NoteAttachments
                .AsNoTracking()
                .Where(a => a.NoteId == noteId)
                .OrderByDescending(a => a.CreatedTime)
                .ToListAsync();

            return list.Select(Map).ToList();
        }

        public async Task<NoteAttachment> GetForDownloadAsync(int userId, int attachmentId)
        {
            var attachment = await _db.NoteAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null)
                throw new KeyNotFoundException("附件不存在。");

            var canAccess = await CanAccessNoteAsync(userId, attachment.NoteId);
            if (!canAccess)
                throw new PermissionDeniedException("无权访问该附件。");

            return attachment;
        }

        /// <summary>
        /// 获取已发布内容的附件（允许匿名访问）。
        /// </summary>
        public async Task<NoteAttachment> GetForPublicDownloadAsync(int attachmentId)
        {
            var attachment = await _db.NoteAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null)
                throw new KeyNotFoundException("附件不存在。");

            var isPublished = await _db.PublicContents
                .AnyAsync(pc => pc.NoteId == attachment.NoteId && pc.Status == PublicContentStatus.Published);

            if (!isPublished)
                throw new PermissionDeniedException("无权访问该附件。");

            return attachment;
        }

        public async Task<NoteAttachment> DeleteAsync(int userId, int attachmentId)
        {
            var attachment = await _db.NoteAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null)
                throw new KeyNotFoundException("附件不存在。");

            var note = await GetNoteForEditAsync(userId, attachment.NoteId);

            _db.NoteAttachments.Remove(attachment);

            if (TryRemoveAttachmentReference(note.ContentJson, attachment.Id, out var updatedJson))
            {
                note.ContentJson = updatedJson;
                note.LastUpdateTime = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return attachment;
        }

        private IQueryable<int> GetAccessibleWorkspaceIds(int userId)
        {
            return _db.WorkspaceMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.WorkspaceId)
                .Union(_db.Workspaces
                    .Where(w => w.OwnerUserId == userId)
                    .Select(w => w.Id));
        }

        private Task<bool> CanAccessNoteAsync(int userId, int noteId)
        {
            var workspaceIds = GetAccessibleWorkspaceIds(userId);
            return _db.Notes.AnyAsync(n => n.Id == noteId && !n.IsDeleted && workspaceIds.Contains(n.WorkspaceId));
        }

        private async Task<Note> GetNoteForEditAsync(int userId, int noteId)
        {
            var note = await _db.Notes
                .Include(n => n.Workspace)
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
                throw new KeyNotFoundException("笔记不存在或无权限访问。");

            if (note.IsDeleted)
                throw new BusinessException("该笔记已在回收站中，无法修改。");

            var canEdit = note.Workspace.OwnerUserId == userId ||
                await _db.WorkspaceMembers.AnyAsync(m =>
                    m.WorkspaceId == note.WorkspaceId &&
                    m.UserId == userId &&
                    m.CanEdit);

            if (!canEdit)
                throw new PermissionDeniedException("无权编辑该笔记。");

            return note;
        }

        private static NoteAttachmentDto Map(NoteAttachment attachment)
        {
            return new NoteAttachmentDto(
                attachment.Id,
                attachment.NoteId,
                attachment.OriginalFileName,
                attachment.ContentType,
                attachment.Size,
                $"/api/notes/attachments/{attachment.Id}",
                attachment.CreatedTime);
        }

        private static bool TryRemoveAttachmentReference(string contentJson, int attachmentId, out string updatedJson)
        {
            updatedJson = contentJson;
            if (string.IsNullOrWhiteSpace(contentJson))
                return false;

            JsonNode? root;
            try
            {
                root = JsonNode.Parse(contentJson);
            }
            catch
            {
                return false;
            }

            if (root == null)
                return false;

            var token = $"/api/notes/attachments/{attachmentId}";
            var altToken = $"api/notes/attachments/{attachmentId}";

            if (root is JsonValue value && value.TryGetValue<string>(out var rootStr))
            {
                var cleaned = CleanString(rootStr, token, altToken);
                if (cleaned == rootStr)
                    return false;

                updatedJson = JsonSerializer.Serialize(cleaned);
                return true;
            }

            var changed = ReplaceTokens(root, token, altToken);
            if (!changed)
                return false;

            updatedJson = root.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            return true;
        }

        private static bool ReplaceTokens(JsonNode node, string token, string altToken)
        {
            bool changed = false;

            if (node is JsonObject obj)
            {
                foreach (var item in obj.ToList())
                {
                    var child = item.Value;
                    if (child is JsonValue value && value.TryGetValue<string>(out var str))
                    {
                        var cleaned = CleanString(str, token, altToken);
                        if (cleaned != str)
                        {
                            obj[item.Key] = cleaned;
                            changed = true;
                        }
                    }
                    else if (child != null)
                    {
                        if (ReplaceTokens(child, token, altToken))
                            changed = true;
                    }
                }
            }
            else if (node is JsonArray array)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    var child = array[i];
                    if (child is JsonValue value && value.TryGetValue<string>(out var str))
                    {
                        var cleaned = CleanString(str, token, altToken);
                        if (cleaned != str)
                        {
                            array[i] = cleaned;
                            changed = true;
                        }
                    }
                    else if (child != null)
                    {
                        if (ReplaceTokens(child, token, altToken))
                            changed = true;
                    }
                }
            }

            return changed;
        }

        private static string CleanString(string input, string token, string altToken)
        {
            var result = input;

            result = MarkdownImageRegex.Replace(result, m => ShouldRemove(m.Groups[1].Value, token, altToken) ? string.Empty : m.Value);
            result = MarkdownLinkRegex.Replace(result, m => ShouldRemove(m.Groups[1].Value, token, altToken) ? string.Empty : m.Value);
            result = HtmlImageRegex.Replace(result, m => ShouldRemove(m.Groups[1].Value, token, altToken) ? string.Empty : m.Value);
            result = HtmlLinkRegex.Replace(result, m => ShouldRemove(m.Groups[1].Value, token, altToken) ? string.Empty : m.Value);

            result = result.Replace(token, string.Empty);
            result = result.Replace(altToken, string.Empty);

            return result;
        }

        private static bool ShouldRemove(string url, string token, string altToken)
        {
            return url.Contains(token, StringComparison.OrdinalIgnoreCase)
                || url.Contains(altToken, StringComparison.OrdinalIgnoreCase);
        }
    }
}
