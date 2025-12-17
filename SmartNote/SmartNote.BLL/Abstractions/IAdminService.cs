using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    /// <summary>
    /// 管理端基础服务契约（后续可扩展用户/工作区/邀请等管理能力）。
    /// </summary>
    public interface IAdminService
    {
        /// <summary>
        /// 健康检查或占位调用，表明管理端服务正常。
        /// </summary>
        Task<AdminPingResult> PingAsync();
    }
}
