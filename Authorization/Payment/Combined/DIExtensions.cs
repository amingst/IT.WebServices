using IT.WebServices.Authorization.Payment.Paypal;
using IT.WebServices.Authorization.Payment.Paypal.Clients;
using IT.WebServices.Authorization.Payment.Paypal.Data;
using IT.WebServices.Authorization.Payment.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddPaymentClasses(this IServiceCollection services)
        {
            services.AddManualPaymentClasses();
            services.AddParallelEconomyClasses();
            services.AddPaypalClasses();
            services.AddStripeClasses();

            return services;
        }

        public static void MapPaymentGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapManualPaymentGrpcServices();
            endpoints.MapParallelEconomyGrpcServices();
            endpoints.MapPaypalGrpcServices();
            endpoints.MapStripeGrpcServices();

            endpoints.MapGrpcService<ClaimsService>();
            endpoints.MapGrpcService<PaymentService>();
            endpoints.MapGrpcService<ServiceOpsService>();
        }
    }
}
