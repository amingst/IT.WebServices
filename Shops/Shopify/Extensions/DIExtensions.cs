using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Shops.Shopify.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ShopifySharp;
using ShopifySharp.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddShopifyClasses(this IServiceCollection services)
        {
            services.AddShopsBaseClasses();
            services.AddShopifySharp<LeakyBucketExecutionPolicy>(ServiceLifetime.Singleton);
            return services;
        }

        public static void MapShopifyGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<ShopifyShopsService>();
        }
    }
}
