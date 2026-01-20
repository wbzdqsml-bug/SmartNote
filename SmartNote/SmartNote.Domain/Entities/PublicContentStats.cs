namespace SmartNote.Domain.Entities
{
    public class PublicContentStats
    {
        public int PublicContentId { get; set; }
        public PublicContent PublicContent { get; set; } = null!;

        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
        public long FavoriteCount { get; set; }
        public long CloneCount { get; set; }
    }
}
