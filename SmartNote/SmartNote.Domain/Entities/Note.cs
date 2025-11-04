namespace SmartNote.Domain.Entities
{
    public class Note
    {
        public int Id { get; set; }

        // 所属工作区
        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        // 内容
        public string Title { get; set; } = string.Empty;
        public string? ContentMd { get; set; }
        public string? ContentHtml { get; set; }
        public string? CanvasDataJson { get; set; }

        // 类型
        public Enums.NoteType Type { get; set; }

        // 删除标记与时间（软删除用）
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedTime { get; set; }

        // 通用时间戳
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
    }
}
