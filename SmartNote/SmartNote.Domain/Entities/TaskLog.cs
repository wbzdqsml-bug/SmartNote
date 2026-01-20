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

        public TaskStatus FromStatus { get; set; }
        public TaskStatus ToStatus { get; set; }

        public int? FromSortOrder { get; set; }
        public int? ToSortOrder { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    }
}
