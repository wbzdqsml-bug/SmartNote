namespace SmartNote.Domain.Enums
{
    /// <summary>
    /// 好友请求/关系状态
    /// </summary>
    public enum FriendshipStatus
    {
        /// <summary>
        /// 申请中
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// 已接受（好友）
        /// </summary>
        Accepted = 1,
        
        /// <summary>
        /// 已拒绝
        /// </summary>
        Rejected = 2,
        
        /// <summary>
        /// 已拉黑
        /// </summary>
        Blocked = 3
    }
}
