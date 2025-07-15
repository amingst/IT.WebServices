using IT.WebServices.Authorization.Payment.Paypal;
using IT.WebServices.Authorization.Payment.Paypal.Clients;
using IT.WebServices.Authorization.Payment.Paypal.Helpers;
using IT.WebServices.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddPaypalClasses(this IServiceCollection services)
        {
            services.AddPaymentBaseClasses();
            services.AddSettingsHelpers();

            services.AddSingleton<SettingsHelper>();

            services.AddSingleton<ReconcileHelper>();

            services.AddSingleton<PaypalClient>();

            return services;
        }

        public static void MapPaypalGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<PaypalService>();
        }
    }
}
