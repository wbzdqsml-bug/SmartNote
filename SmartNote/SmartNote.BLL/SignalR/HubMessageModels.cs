namespace SmartNote.BLL.SignalR
{
    /// <summary>
    /// 与 NoteHub 交互的消息契约，便于前后端统一。
    /// </summary>
    public record NoteContentUpdate(int NoteId, int UserId, string ContentJson, DateTime Timestamp);

    /// <summary>
    /// 实时在线状态/进入离开通知
    /// </summary>
    public record NotePresence(int NoteId, int UserId, string Username, bool Online, DateTime Timestamp);

    /// <summary>
    /// 通用通知消息（如笔记被删除/权限变更）
    /// </summary>
    public record NoteNotification(int NoteId, string Message, DateTime Timestamp);
}
