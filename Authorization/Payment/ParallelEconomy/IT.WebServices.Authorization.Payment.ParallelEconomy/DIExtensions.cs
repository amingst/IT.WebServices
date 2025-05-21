using IT.WebServices.Authorization.Payment.ParallelEconomy;
using IT.WebServices.Authorization.Payment.ParallelEconomy.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddManualPaymentClasses(this IServiceCollection services)
        {
            services.AddSingleton<IPaymentRecordProvider, SqlPaymentRecordProvider>();
            services.AddSingleton<ISubscriptionRecordProvider, SqlSubscriptionRecordProvider>();
            services.AddSingleton<ISubscriptionFullRecordProvider, SubscriptionFullRecordProvider>();

            return services;
        }

        public static void MapManualPaymentGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<BackupService>();
            endpoints.MapGrpcService<ParallelEconomyService>();
        }
    }
}
