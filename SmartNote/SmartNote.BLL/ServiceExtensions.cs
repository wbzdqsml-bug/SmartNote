using Microsoft.Extensions.DependencyInjection;
using SmartNote.BLL.Abstractions;
using SmartNote.BLL.Services;

namespace SmartNote.BLL
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }
    }
}
