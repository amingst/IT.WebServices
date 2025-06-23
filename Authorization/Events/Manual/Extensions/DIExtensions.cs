using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Authorization.Events.Manual.Data;
using IT.WebServices.Authorization.Events.Manual.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace IT.WebServices.Authorization.Events.Manual.Extensions
{
    public static class DIExtensions
    {
        public static IServiceCollection AddManualEventsClasses(this IServiceCollection services)
        {
            services.AddSingleton<IEventDataProvider, FileSystemEventDataProvider>();
            services.AddSingleton<ITicketDataProvider, FileSystemTicketDataProvider>();
            services.AddSingleton<IRSVPDataProvider, FileSystemEventRSVPDataProvider>();
            return services;
        }

        public static void MapManualEventsGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<ManualEventsService>();
        }
    }
}
