namespace SmartNote.Domain.Enums
{
    /// <summary>
    /// 聊天消息类型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 纯文本
        /// </summary>
        Text = 0,
        
        /// <summary>
        /// 图片
        /// </summary>
        Image = 1,
        
        /// <summary>
        /// 文件
        /// </summary>
        File = 2,
        
        /// <summary>
        /// 系统消息（如：xxx加入群聊）
        /// </summary>
        System = 3
    }
}
