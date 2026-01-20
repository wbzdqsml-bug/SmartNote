using SmartNote.Domain.Entities.Enums;

namespace SmartNote.Domain.Entities
{
    public class TaskLog
    {
        public int Id { get; set; }

        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; } = null!;

        public int ActorUserId { get; set; }
        public User ActorUser { get; set; } = null!;

        public TaskItemStatus FromStatus { get; set; }
        public TaskItemStatus ToStatus { get; set; }

        public int? FromSortOrder { get; set; }
        public int? ToSortOrder { get; set; }

        public string Action { get; set; } = string.Empty;
        public string? PayloadJson { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    }
}
