using System;

namespace SmartNote.Domain.Entities
{
    public class NoteAttachment
    {
        public int Id { get; set; }

        public int NoteId { get; set; }
        public Note Note { get; set; } = null!;

        public int UploaderUserId { get; set; }
        public User UploaderUser { get; set; } = null!;

        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public long Size { get; set; }

        public string StoragePath { get; set; } = string.Empty;

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
