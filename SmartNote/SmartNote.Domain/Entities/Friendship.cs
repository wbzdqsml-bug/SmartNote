using SmartNote.Domain.Enums;
using System;

namespace SmartNote.Domain.Entities
{
    /// <summary>
    /// 好友关系实体
    /// </summary>
    public class Friendship
    {
        public int Id { get; set; }

        /// <summary>
        /// 发起申请的用户 ID
        /// </summary>
        public int RequesterId { get; set; }
        public virtual User Requester { get; set; } = null!;

        /// <summary>
        /// 接收申请的用户 ID
        /// </summary>
        public int AddresseeId { get; set; }
        public virtual User Addressee { get; set; } = null!;

        /// <summary>
        /// 当前状态
        /// </summary>
        public FriendshipStatus Status { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime RequestTime { get; set; }

        /// <summary>
        /// 处理时间（接受/拒绝时间）
        /// </summary>
        public DateTime? ResponseTime { get; set; }
    }
}
