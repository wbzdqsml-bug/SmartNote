using SmartNote.Domain.Entities.Enums;

namespace SmartNote.Shared.Dtos
{
    public class PublicContentListItemDto
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public PublicContentType ContentType { get; set; }
        public PublicContentStatus Status { get; set; }
        public int AuthorUserId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
        public long FavoriteCount { get; set; }
        public long CloneCount { get; set; }
    }

    public class PublicContentDetailDto
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentJson { get; set; } = "{}";
        public PublicContentType ContentType { get; set; }
        public PublicContentStatus Status { get; set; }
        public int AuthorUserId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
        public long FavoriteCount { get; set; }
        public long CloneCount { get; set; }
    }

    public class PublicContentPageDto
    {
        public int TotalCount { get; set; }
        public List<PublicContentListItemDto> Items { get; set; } = new();
    }

    public class PublicCommentDto
    {
        public int Id { get; set; }
        public int PublicContentId { get; set; }
        public int AuthorUserId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; }
    }

    public class PublicCommentCreateDto
    {
        public int PublicContentId { get; set; }
        public int? ParentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class PublicContentCloneRequest
    {
        public int PublicContentId { get; set; }
        public int WorkspaceId { get; set; }
        public string? Title { get; set; }
    }

    public class PublicContentReactionRequest
    {
        public int PublicContentId { get; set; }
        public bool IsLiked { get; set; }
        public bool IsFavorite { get; set; }
    }
}
