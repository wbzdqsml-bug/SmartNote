using SmartNote.Domain.Entities.Enums;

namespace SmartNote.Shared.Dtos
{
    public class TaskCreateDto
    {
        public int WorkspaceId { get; set; }
        public int? NoteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
        public int SortOrder { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? DueAt { get; set; }
    }

    public class TaskUpdateDto
    {
        public int? NoteId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TaskItemStatus? Status { get; set; }
        public int? SortOrder { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? DueAt { get; set; }
    }

    public class TaskViewDto
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; }
        public int? NoteId { get; set; }
        public int OwnerUserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskItemStatus Status { get; set; }
        public int SortOrder { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    public class TaskSortUpdateItemDto
    {
        public int TaskId { get; set; }
        public TaskItemStatus Status { get; set; }
        public int SortOrder { get; set; }
    }

    public class TaskSortUpdateRequest
    {
        public List<TaskSortUpdateItemDto> Items { get; set; } = new();
    }
}
