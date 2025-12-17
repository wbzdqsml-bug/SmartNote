using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Services
{
    public class AdminService : IAdminService
    {
        public Task<AdminPingResult> PingAsync()
        {
            var result = new AdminPingResult(
                "admin",
                "管理端服务占位，后续可扩展实际业务。",
                DateTime.UtcNow);

            return Task.FromResult(result);
        }
    }
}
