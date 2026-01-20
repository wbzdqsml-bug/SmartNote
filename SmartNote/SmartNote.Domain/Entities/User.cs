﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; } = new List<WorkspaceMember>();
        public UserProfile? Profile { get; set; }
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<NoteActivityLog> NoteActivityLogs { get; set; } = new List<NoteActivityLog>();
        
        // 聊天与好友相关导航属性
        public ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();
        public ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();
        public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
        public ICollection<ChatMessage> ReceivedMessages { get; set; } = new List<ChatMessage>();
        public ICollection<PublicContent> PublicContents { get; set; } = new List<PublicContent>();
        public ICollection<PublicComment> PublicComments { get; set; } = new List<PublicComment>();
        public ICollection<PublicContentReaction> PublicContentReactions { get; set; } = new List<PublicContentReaction>();
        public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
        public ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();
    }
}
