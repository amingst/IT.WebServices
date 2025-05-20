using IT.WebServices.Authorization.Payment.Manual;
using IT.WebServices.Authorization.Payment.Manual.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddManualPaymentClasses(this IServiceCollection services)
        {
            services.AddSingleton<ISubscriptionRecordProvider, SqlManualDataProvider>();

            return services;
        }

        public static void MapManualPaymentGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<ManualPaymentService>();
        }
    }
}
