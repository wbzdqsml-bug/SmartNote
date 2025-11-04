namespace SmartNote.Shared.Dtos
{
    public class WorkspaceInvitationViewDto
    {
        public int InvitationId { get; set; }
        public int WorkspaceId { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;

        public int InviterUserId { get; set; }
        public string InviterUsername { get; set; } = string.Empty;

        public bool CanEdit { get; set; }
        public bool CanShare { get; set; }

        public string Status { get; set; } = "Pending";
        public string? Message { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime? RespondedTime { get; set; }
    }

    public class WorkspaceInvitationSendDto
    {
        public string InviteeUsername { get; set; } = string.Empty;
        public bool CanEdit { get; set; }
        public bool CanShare { get; set; }
        public string? Message { get; set; }
    }
}
