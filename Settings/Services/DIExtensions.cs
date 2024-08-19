using IT.WebServices.Settings;
using IT.WebServices.Settings.Services;
using IT.WebServices.Settings.Services.Data;
using IT.WebServices.Settings.Services.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddSettingsClasses(this IServiceCollection services)
        {
            services.AddSingleton<ISettingsDataProvider, FileSettingsDataProvider>();
            services.AddSingleton<OfflineHelper>();

            services.AddScoped<ISettingsService, SettingsService>();

            return services;
        }

        public static void MapSettingsGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            //endpoints.MapGrpcService<BackupService>();
            endpoints.MapGrpcService<SettingsService>();
        }
    }
}
