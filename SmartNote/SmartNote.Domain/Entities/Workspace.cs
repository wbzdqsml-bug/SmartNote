using SmartNote.Domain.Entities.Enums;

namespace SmartNote.Domain.Entities
{
    public class Workspace
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public WorkspaceType Type { get; set; }
        public int OwnerUserId { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();

    }
}
