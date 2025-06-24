using IT.WebServices.Authorization.Payment.Paypal;
using IT.WebServices.Authorization.Payment.Paypal.Clients;
using IT.WebServices.Authorization.Payment.Paypal.Data;
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
            services.AddSettingsHelpers();

            services.AddSingleton<MySQLHelper>();
            services.AddSingleton<SettingsHelper>();

            services.AddSingleton<BulkHelper>();
            services.AddSingleton<ReconcileHelper>();

            services.AddSingleton<PaypalClient>();
            services.AddSingleton<ISubscriptionRecordProvider, SqlSubscriptionRecordProvider>();
            services.AddSingleton<ISubscriptionFullRecordProvider, SubscriptionFullRecordProvider>();
            services.AddSingleton<IPaymentRecordProvider, SqlPaymentRecordProvider>();

            return services;
        }

        public static void MapPaypalGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<BackupService>();
            endpoints.MapGrpcService<PaypalService>();
        }
    }
}
