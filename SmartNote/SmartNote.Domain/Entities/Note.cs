using SmartNote.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Domain.Entities
{
    public class Note
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public NoteType Type { get; set; } = NoteType.Markdown;
        public string? ContentMd { get; set; }
        public string? ContentHtml { get; set; }
        public string? CanvasDataJson { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
    }
}
