using IT.WebServices.Authorization.Payment.Combined.Services;
using IT.WebServices.Authorization.Payment.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddPaymentClasses(this IServiceCollection services)
        {
            services.AddManualPaymentClasses();
            services.AddFortisClasses();
            services.AddPaypalClasses();
            services.AddStripeClasses();

            services.AddSingleton<BulkHelper>();

            return services;
        }

        public static void MapPaymentGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapManualPaymentGrpcServices();
            endpoints.MapFortisGrpcServices();
            endpoints.MapPaypalGrpcServices();
            endpoints.MapStripeGrpcServices();

            endpoints.MapGrpcService<AdminPaymentService>();
            endpoints.MapGrpcService<ClaimsService>();
            endpoints.MapGrpcService<PaymentService>();
            endpoints.MapGrpcService<ServiceOpsService>();
        }
    }
}
