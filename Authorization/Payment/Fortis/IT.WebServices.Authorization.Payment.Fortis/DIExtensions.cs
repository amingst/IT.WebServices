using IT.WebServices.Authorization.Payment.Fortis;
using IT.WebServices.Authorization.Payment.Fortis.Clients;
using IT.WebServices.Authorization.Payment.Fortis.Data;
using IT.WebServices.Authorization.Payment.Fortis.Helpers;
using IT.WebServices.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddFortisClasses(this IServiceCollection services)
        {
            services.AddSettingsHelpers();

            services.AddSingleton<MySQLHelper>();
            services.AddSingleton<SettingsHelper>();

            services.AddSingleton<BulkHelper>();
            services.AddSingleton<ReconcileHelper>();
            services.AddSingleton<FortisClient>();
            services.AddSingleton<FortisContactHelper>();
            services.AddSingleton<FortisSubscriptionHelper>();
            services.AddSingleton<FortisTokenHelper>();
            services.AddSingleton<FortisTransactionHelper>();

            services.AddSingleton<IPaymentRecordProvider, SqlPaymentRecordProvider>();
            services.AddSingleton<ISubscriptionRecordProvider, SqlSubscriptionRecordProvider>();
            services.AddSingleton<ISubscriptionFullRecordProvider, SubscriptionFullRecordProvider>();

            return services;
        }

        public static void MapFortisGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<BackupService>();
            endpoints.MapGrpcService<FortisService>();
        }
    }
}
