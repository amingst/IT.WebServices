using IT.WebServices.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SettingsExtensions
    {
        public static void AddSettingsHelpers(this IServiceCollection services)
        {
            services.AddSingleton<CategoryHelper>();
            services.AddSingleton<ChannelHelper>();
            services.AddSingleton<ServiceNameHelper>();
            services.AddSingleton<SettingsClient>();
            services.AddSingleton<SubscriptionTierHelper>();
        }
    }
}
