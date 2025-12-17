using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartNote.BLL.SignalR;
using SmartNote.Common.Extensions;

namespace SmartNote.WebAPI.User.Hubs
{
    /// <summary>
    /// 简单的笔记协作 Hub：加入/离开房间、广播内容、同步在线状态。
    /// </summary>
    [Authorize]
    public class NoteHub : Hub<INoteHubClient>
    {
        public async Task JoinNote(int noteId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, NoteHubGroups.NoteRoom(noteId));

            var presence = new NotePresence(
                noteId,
                Context.User!.GetUserId(),
                Context.User!.GetUsername(),
                true,
                DateTime.UtcNow);

            await Clients.Group(NoteHubGroups.NoteRoom(noteId)).PresenceChanged(presence);
        }

        public async Task LeaveNote(int noteId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, NoteHubGroups.NoteRoom(noteId));

            var presence = new NotePresence(
                noteId,
                Context.User!.GetUserId(),
                Context.User!.GetUsername(),
                false,
                DateTime.UtcNow);

            await Clients.Group(NoteHubGroups.NoteRoom(noteId)).PresenceChanged(presence);
        }

        /// <summary>
        /// 广播笔记内容更新，强制使用当前用户 Id 以防伪造。
        /// </summary>
        public async Task BroadcastContent(NoteContentUpdate update)
        {
            var payload = update with
            {
                UserId = Context.User!.GetUserId(),
                Timestamp = DateTime.UtcNow
            };

            await Clients
                .OthersInGroup(NoteHubGroups.NoteRoom(update.NoteId))
                .ContentUpdated(payload);
        }
    }
}
