using IT.WebServices.Authentication.Services;
using IT.WebServices.Authentication.Services.Data;
using IT.WebServices.Authentication.Services.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using static Google.Rpc.Context.AttributeContext.Types;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddAuthenticationClasses(this IServiceCollection services)
        {
            services.AddSingleton<IUserDataProvider, FileSystemUserDataProvider>();
            services.AddScoped<ClaimsClient>();
            services.AddSingleton<OfflineHelper>();

            return services;
        }

        public static void MapAuthenticationGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            //endpoints.MapGrpcService<BackupService>();
            endpoints.MapGrpcService<ServiceService>();
            endpoints.MapGrpcService<UserService>();
        }
    }
}
