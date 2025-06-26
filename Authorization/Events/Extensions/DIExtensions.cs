using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Authorization.Events.Data;
using IT.WebServices.Authorization.Events.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace IT.WebServices.Authorization.Events.Extensions
{
    public static class DIExtensions
    {
        public static IServiceCollection AddEventsClasses(this IServiceCollection services)
        {
            services.AddSingleton<IEventDataProvider, FileSystemEventDataProvider>();
            services.AddSingleton<ITicketDataProvider, FileSystemTicketDataProvider>();
            services.AddSingleton<IRSVPDataProvider, FileSystemEventRSVPDataProvider>();
            services.AddSingleton<
                IEventInstanceOverrideDataProvider,
                FileSystemEventInstanceOverrideDataProvider
            >();
            return services;
        }

        public static void MapEventsGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<EventsService>();
            endpoints.MapGrpcService<EventTicketService>();
        }
    }
}
