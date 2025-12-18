using SmartNote.Domain.Enums;
using System;

namespace SmartNote.Domain.Entities
{
    /// <summary>
    /// 聊天消息实体（支持私聊和群聊）
    /// </summary>
    public class ChatMessage
    {
        public int Id { get; set; }

        /// <summary>
        /// 发送者 ID
        /// </summary>
        public int SenderId { get; set; }
        public virtual User Sender { get; set; } = null!;

        /// <summary>
        /// 接收者 ID（仅私聊时有值）
        /// </summary>
        public int? ReceiverId { get; set; }
        public virtual User? Receiver { get; set; }

        /// <summary>
        /// 工作区 ID（仅群聊时有值）
        /// </summary>
        public int? WorkspaceId { get; set; }
        public virtual Workspace? Workspace { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType Type { get; set; } = MessageType.Text;

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SentAt { get; set; }

        /// <summary>
        /// 是否已读（主要用于私聊）
        /// </summary>
        public bool IsRead { get; set; }
    }
}
