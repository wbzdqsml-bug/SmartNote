using SmartNote.Domain.Enums;

namespace SmartNote.Shared.Dtos
{
    public record ChatMessageDto(int Id, int SenderId, string? SenderName, string Content, DateTime SentAt, MessageType Type);
}