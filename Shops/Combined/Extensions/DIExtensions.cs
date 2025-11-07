using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Shops.Combined.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddShopsClasses(this IServiceCollection services)
        {
            services.AddShopifyClasses();
            return services;
        }

        public static void MapShopsGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapShopifyGrpcServices();
            endpoints.MapGrpcService<ShopsService>();
        }
    }
}
