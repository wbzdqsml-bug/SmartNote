using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Enums;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _db;

        public ChatService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ChatMessageDto>> GetPrivateHistoryAsync(int userId, int friendId)
        {
            return await _db.ChatMessages
                .Where(m => (m.SenderId == userId && m.ReceiverId == friendId) ||
                            (m.SenderId == friendId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .Take(100)
                .Select(m => new ChatMessageDto(
                    m.Id, m.SenderId, null, m.Content, m.SentAt, m.Type
                ))
                .ToListAsync();
        }

        public async Task<List<ChatMessageDto>> GetWorkspaceHistoryAsync(int userId, int workspaceId)
        {
            var isMember = await _db.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);

            if (!isMember) throw new BusinessException("你不是该工作区的成员");

            return await _db.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.WorkspaceId == workspaceId)
                .OrderBy(m => m.SentAt)
                .Take(100)
                .Select(m => new ChatMessageDto(
                    m.Id, m.SenderId, m.Sender.Username, m.Content, m.SentAt, m.Type
                ))
                .ToListAsync();
        }

        public async Task<ChatMessageDto> SavePrivateMessageAsync(int senderId, int receiverId, string content)
        {
            var msg = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow,
                Type = MessageType.Text
            };
            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            return new ChatMessageDto(msg.Id, msg.SenderId, null, msg.Content, msg.SentAt, msg.Type);
        }

        public async Task<ChatMessageDto> SaveWorkspaceMessageAsync(int senderId, int workspaceId, string content)
        {
            var isMember = await _db.WorkspaceMembers.AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == senderId);
            if (!isMember) throw new BusinessException("你不是该工作区成员");

            var msg = new ChatMessage
            {
                SenderId = senderId,
                WorkspaceId = workspaceId,
                Content = content,
                SentAt = DateTime.UtcNow,
                Type = MessageType.Text
            };
            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            return new ChatMessageDto(msg.Id, msg.SenderId, null, msg.Content, msg.SentAt, msg.Type);
        }

        public async Task<List<int>> GetUserWorkspaceIdsAsync(int userId)
        {
            return await _db.WorkspaceMembers
                .Where(wm => wm.UserId == userId)
                .Select(wm => wm.WorkspaceId)
                .ToListAsync();
        }
    }
}