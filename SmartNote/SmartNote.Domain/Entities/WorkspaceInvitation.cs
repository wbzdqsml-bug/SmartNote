using SmartNote.Domain.Entities.Enums;

namespace SmartNote.Domain.Entities
{
    public class WorkspaceInvitation
    {
        public int Id { get; set; }

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public int InviterUserId { get; set; }   // 邀请发起人
        public User InviterUser { get; set; } = null!;

        public int InviteeUserId { get; set; }   // 被邀请人
        public User InviteeUser { get; set; } = null!;

        public bool CanEdit { get; set; } = false;
        public bool CanShare { get; set; } = false;

        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
        public string? Message { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedTime { get; set; }
    }
}
