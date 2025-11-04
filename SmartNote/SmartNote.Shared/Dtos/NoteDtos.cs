using SmartNote.Domain.Entities.Enums;
namespace SmartNote.Shared.Dtos
{
    public class NoteCreateDto
    {
        public int WorkspaceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ContentMd { get; set; }
        public string? ContentHtml { get; set; }
        public string? CanvasDataJson { get; set; }  // 手写笔记数据
        public NoteType Type { get; set; } 
    }

    public class NoteUpdateDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ContentMd { get; set; }
        public string? ContentHtml { get; set; }
        public string? CanvasDataJson { get; set; }
    }

    namespace SmartNote.Shared.Dtos
    {
        public class NoteViewDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? ContentHtml { get; set; }
            public string? ContentMd { get; set; }
            public string? CanvasDataJson { get; set; }
            public int WorkspaceId { get; set; }   // 所属工作区
            public bool IsDeleted { get; set; }    // 是否被软删除
            public DateTime? DeletedTime { get; set; } // 删除时间

            public DateTime CreateTime { get; set; }
            public DateTime LastUpdateTime { get; set; }
        }
    }

}
