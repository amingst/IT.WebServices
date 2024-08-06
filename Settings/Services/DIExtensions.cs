using IT.WebServices.Settings.Services;
using IT.WebServices.Settings.Services.Data;
using IT.WebServices.Settings.Services.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddSettingsClasses(this IServiceCollection services)
        {
            services.AddSingleton<ISettingsDataProvider, FileSettingsDataProvider>();
            services.AddSingleton<OfflineHelper>();

            return services;
        }

        public static void MapSettingsGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            //endpoints.MapGrpcService<BackupService>();
            endpoints.MapGrpcService<SettingsService>();
        }
    }
}
