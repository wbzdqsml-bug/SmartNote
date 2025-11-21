using SmartNote.Domain.Entities.Enums;

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

        public string ContentJson { get; set; } = "{}"; // 统一笔记内容
        // 类型
        public NoteType Type { get; set; }

        // 删除标记与时间（软删除用）
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedTime { get; set; }

        // 通用时间戳
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 所属分类（可为空）
        /// </summary>
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        /// <summary>
        /// 标签多对多关系
        /// </summary>
        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
    }
}
