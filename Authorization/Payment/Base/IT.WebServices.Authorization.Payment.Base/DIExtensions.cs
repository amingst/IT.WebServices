using IT.WebServices.Authorization.Payment.Generic.Data;
using IT.WebServices.Helpers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddPaymentBaseClasses(this IServiceCollection services)
        {
            services.AddSettingsHelpers();

            services.AddSingleton<MySQLHelper>();

            services.AddSingleton<IGenericPaymentRecordProvider, SqlPaymentRecordProvider>();
            services.AddSingleton<IGenericSubscriptionRecordProvider, SqlSubscriptionRecordProvider>();
            services.AddSingleton<IGenericSubscriptionFullRecordProvider, SubscriptionFullRecordProvider>();

            return services;
        }
    }
}
