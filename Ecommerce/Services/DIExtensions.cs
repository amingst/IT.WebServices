using IT.WebServices.Ecommerce;
using IT.WebServices.Ecommerce.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddEcommerceClasses(this IServiceCollection services)
        {
            // services.AddShopifySharp<LeakyBucketExecutionPolicy>();
            services.AddScoped<IEcommerceService, EcommerceService>();
            return services;
        }

        public static void MapEcommerceGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<EcommerceService>();
        }
    }
}
