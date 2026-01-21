namespace SmartNote.Domain.Entities
{
    public class PublicComment
    {
        public int Id { get; set; }

        public int PublicContentId { get; set; }
        public PublicContent PublicContent { get; set; } = null!;

        public int AuthorUserId { get; set; }
        public User AuthorUser { get; set; } = null!;

        public int? ParentId { get; set; }
        public PublicComment? Parent { get; set; }

        public ICollection<PublicComment> Replies { get; set; } = new List<PublicComment>();

        public string Content { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    }
}
