using SmartNote.Domain.Entities.Enums;

namespace SmartNote.Shared.Dtos
{
    /// <summary>
    /// 创建笔记 DTO
    /// </summary>
    public class NoteCreateDto
    {
        public int WorkspaceId { get; set; }

        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 笔记类型（Markdown / Canvas / MindMap / RichText）
        /// </summary>
        public NoteType Type { get; set; } = NoteType.Markdown;
    }

    /// <summary>
    /// 更新笔记 DTO
    /// </summary>
    public class NoteUpdateDto
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        /// <summary>
        /// 内容 JSON（由前端直接传完整内容结构）
        /// </summary>
        public string? ContentJson { get; set; }
    }

    /// <summary>
    /// 笔记视图 DTO（输出到前端）
    /// </summary>
    public class NoteViewDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public NoteType Type { get; set; } = NoteType.Markdown;

        public string ContentJson { get; set; } = "{}";

        public int WorkspaceId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedTime { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}
