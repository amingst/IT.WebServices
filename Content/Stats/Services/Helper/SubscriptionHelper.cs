using Microsoft.AspNetCore.Builder;
using IT.WebServices.Authentication;
using IT.WebServices.Content.Stats.Services.Subscriptions;

namespace IT.WebServices.Content.Stats.Services.Helper
{
    public static class SubscriptionHelper
    {
        public static void LaunchSubscriptionServices(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService(typeof(ContentSubscription));
            app.ApplicationServices.GetService(typeof(UserSubscription));
        }
    }
}
