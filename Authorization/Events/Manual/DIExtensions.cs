using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Authorization.Events.Manual.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddManualEventsClasses(this IServiceCollection services)
        {
            return services;
        }

        public static void MapManualEventsGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<ManualEventsService>();
        }
    }
}
