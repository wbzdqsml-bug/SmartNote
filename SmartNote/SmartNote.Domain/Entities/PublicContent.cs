using SmartNote.Domain.Entities.Enums;

namespace SmartNote.Domain.Entities
{
    public class PublicContent
    {
        public int Id { get; set; }

        public int NoteId { get; set; }
        public Note Note { get; set; } = null!;

        public int AuthorUserId { get; set; }
        public User AuthorUser { get; set; } = null!;

        public PublicContentType ContentType { get; set; } = PublicContentType.Note;
        public PublicContentStatus Status { get; set; } = PublicContentStatus.Private;

        public string? TitleSnapshot { get; set; }
        public string? ContentSnapshotJson { get; set; }

        public DateTime? PublishedAt { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        public PublicContentStats Stats { get; set; } = null!;
        public ICollection<PublicComment> Comments { get; set; } = new List<PublicComment>();
        public ICollection<PublicContentReaction> Reactions { get; set; } = new List<PublicContentReaction>();
    }
}
