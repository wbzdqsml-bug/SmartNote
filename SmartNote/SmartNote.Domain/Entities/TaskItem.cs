using SmartNote.Domain.Entities.Enums;

namespace SmartNote.Domain.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public int? NoteId { get; set; }
        public Note? Note { get; set; }

        public int OwnerUserId { get; set; }
        public User OwnerUser { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
        public int SortOrder { get; set; }

        public DateTime? StartAt { get; set; }
        public DateTime? DueAt { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        public ICollection<TaskLog> Logs { get; set; } = new List<TaskLog>();
    }
}
