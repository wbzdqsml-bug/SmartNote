using System;

namespace SmartNote.Shared.Dtos
{
    public record NoteAttachmentDto(
        int Id,
        int NoteId,
        string FileName,
        string ContentType,
        long Size,
        string DownloadUrl,
        DateTime CreatedTime);
}
