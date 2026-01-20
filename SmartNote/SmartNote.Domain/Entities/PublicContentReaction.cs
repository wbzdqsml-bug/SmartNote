namespace SmartNote.Domain.Entities
{
    public class PublicContentReaction
    {
        public int PublicContentId { get; set; }
        public PublicContent PublicContent { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public bool IsFavorite { get; set; }
        public bool IsLiked { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    }
}
