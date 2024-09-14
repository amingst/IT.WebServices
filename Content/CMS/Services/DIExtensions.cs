using IT.WebServices.Content.CMS.Services;
using IT.WebServices.Content.CMS.Services.Data;
using IT.WebServices.Content.CMS.Services.Helpers;
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
        public static IServiceCollection AddCMSClasses(this IServiceCollection services)
        {
            services.AddSingleton<IAssetDataProvider, MemCachedFileSystemAssetDataProvider>();
            services.AddSingleton<IContentDataProvider, SqlContentDataProvider>();
            services.AddSingleton<IPageDataProvider, MemCachedFileSystemPageDataProvider>();
            services.AddSingleton<FileSystemAssetDataProvider>();
            services.AddSingleton<FileSystemContentDataProvider>();
            services.AddSingleton<FileSystemPageDataProvider>();
            services.AddScoped<StatsClient>();

            return services;
        }

        public static void MapCMSGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            //endpoints.MapGrpcService<AssetBackupService>();
            endpoints.MapGrpcService<AssetService>();
            //endpoints.MapGrpcService<BackupService>();
            endpoints.MapGrpcService<ContentService>();
            endpoints.MapGrpcService<PageService>();
        }
    }
}
