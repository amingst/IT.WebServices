using IT.WebServices.Authorization.Payment.ParallelEconomy;
using IT.WebServices.Authorization.Payment.ParallelEconomy.Clients;
using IT.WebServices.Authorization.Payment.ParallelEconomy.Data;
using IT.WebServices.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddParallelEconomyClasses(this IServiceCollection services)
        {
            services.AddSettingsHelpers();

            services.AddSingleton<MySQLHelper>();

            services.AddSingleton<ParallelEconomyClient>();
            services.AddSingleton<IPaymentRecordProvider, SqlPaymentRecordProvider>();
            services.AddSingleton<ISubscriptionRecordProvider, SqlSubscriptionRecordProvider>();
            services.AddSingleton<ISubscriptionFullRecordProvider, SubscriptionFullRecordProvider>();

            return services;
        }

        public static void MapParallelEconomyGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<BackupService>();
            endpoints.MapGrpcService<ParallelEconomyService>();
        }
    }
}
