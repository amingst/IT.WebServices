using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace IT.WebServices.Integrations.Discord.Services;
public static class DIExtensions
{
    public static IServiceCollection AddDiscordIntegrationClasses(this IServiceCollection services)
    {
        return services;
    }

    public static void MapDiscordIntegrationGrpcServices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<DiscordService>();
    }
}
