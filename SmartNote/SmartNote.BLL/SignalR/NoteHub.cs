using System.Threading.Tasks;

namespace SmartNote.BLL.SignalR
{
    /// <summary>
    /// Hub 客户端契约，Server 端调用这些方法通知前端。
    /// </summary>
    public interface INoteHubClient
    {
        Task ContentUpdated(NoteContentUpdate update);
        Task PresenceChanged(NotePresence presence);
        Task Notified(NoteNotification notification);
    }

    /// <summary>
    /// Hub 分组/路径辅助，避免魔法字符串。
    /// </summary>
    public static class NoteHubGroups
    {
        public static string NoteRoom(int noteId) => $"note:{noteId}";
    }
}
