using SmartNote.Domain.Entities;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface INoteAttachmentService
    {
        Task<NoteAttachmentDto> CreateAsync(int userId, int noteId, string originalFileName, string contentType, long size, string storagePath);
        Task<IReadOnlyList<NoteAttachmentDto>> GetByNoteAsync(int userId, int noteId);
        Task<NoteAttachment> GetForDownloadAsync(int userId, int attachmentId);
        Task EnsureCanAccessNoteAsync(int userId, int noteId);
        Task EnsureCanEditNoteAsync(int userId, int noteId);
        Task<NoteAttachment> DeleteAsync(int userId, int attachmentId);
    }
}
