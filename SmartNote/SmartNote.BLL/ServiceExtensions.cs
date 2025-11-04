using Microsoft.Extensions.DependencyInjection;
using SmartNote.BLL.Abstractions;
using SmartNote.BLL.Services;

namespace SmartNote.BLL
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // ✅ 注册 AuthService
            services.AddScoped<IAuthService, AuthService>();

            // ✅ 注册 NoteService
            services.AddScoped<INoteService, NoteService>();
            // 🧩 回收站（缺这个会导致 RecycleController 无法注入）
            services.AddScoped<IRecycleService, RecycleService>();

            // 🧩 工作区
            services.AddScoped<IWorkspaceService, WorkspaceService>();

            services.AddScoped<IWorkspaceMemberService, WorkspaceMemberService>();
            // ✅ 如果后续还要添加其他服务，也统一在这里注册
            // services.AddScoped<IAnalysisService, AnalysisService>();
            services.AddScoped<IWorkspaceInvitationService, WorkspaceInvitationService>();

            return services;
        }
    }
}
