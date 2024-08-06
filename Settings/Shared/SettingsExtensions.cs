using Microsoft.Extensions.DependencyInjection;

namespace IT.WebServices.Settings
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
