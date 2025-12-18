using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartNote.BLL.Abstractions;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _service;

        public ChatHub(IChatService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            var idStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out var id) ? id : 0;
        }

        /// <summary>
        /// 客户端连接时触发
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId > 0)
            {
                // 1. 自动将用户加入他所在的所有工作区 Group
                // 🔒 私密性保证：只有数据库中记录的成员，才会被加入 SignalR 组接收消息
                var workspaceIds = await _service.GetUserWorkspaceIdsAsync(userId);

                foreach (var wid in workspaceIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"workspace-{wid}");
                }
            }
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 发送私聊消息
        /// </summary>
        public async Task SendPrivateMessage(int receiverId, string content)
        {
            var senderId = GetUserId();
            if (string.IsNullOrWhiteSpace(content)) return;

            // 保存到数据库 (通过 Service)
            var msgDto = await _service.SavePrivateMessageAsync(senderId, receiverId, content);

            // 实时推送给接收者 (SignalR 默认使用 UserId 作为 User 标识)
            // 🔒 私密性保证：SignalR 的 Clients.User 只会推送到通过 JWT 认证为该 ID 的连接
            await Clients.User(receiverId.ToString()).SendAsync("ReceivePrivateMessage", new 
            {
                SenderId = senderId,
                Content = content, // 保持与之前一致的匿名对象结构，或者直接传 msgDto
                SentAt = msgDto.SentAt
            });
        }

        /// <summary>
        /// 发送群聊消息
        /// </summary>
        public async Task SendWorkspaceMessage(int workspaceId, string content)
        {
            var senderId = GetUserId();

            // 保存到数据库 (通过 Service，内部会检查权限)
            var msgDto = await _service.SaveWorkspaceMessageAsync(senderId, workspaceId, content);

            // 推送到组
            await Clients.Group($"workspace-{workspaceId}").SendAsync("ReceiveWorkspaceMessage", new
            {
                WorkspaceId = workspaceId,
                SenderId = senderId,
                Content = content,
                SentAt = msgDto.SentAt
            });
        }
    }
}