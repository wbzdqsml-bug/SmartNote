using Microsoft.Extensions.DependencyInjection;
using SmartNote.BLL.Abstractions;
using SmartNote.BLL.Services;

namespace SmartNote.BLL
{
    /// <summary>
    /// 业务层依赖注入扩展。
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// 注册业务层服务依赖。
        /// </summary>
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

            services.AddScoped<IProfileService, ProfileService>();

            services.AddScoped<ICategoryService, CategoryService>();

            services.AddScoped<ITagService, TagService>();

            services.AddScoped<IAnalysisService, AnalysisService>();

            // 管理端占位服务（Admin API 后续实现时可复用）
            services.AddScoped<IAdminService, AdminService>();

            services.AddScoped<INoteAttachmentService, NoteAttachmentService>();

            // AI
            services.AddScoped<IAiNoteService, AiNoteService>();
            services.AddScoped<ICommunityService, CommunityService>();
            services.AddScoped<ITaskService, TaskService>();

            return services;
        }
    }
}
