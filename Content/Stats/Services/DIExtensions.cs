using IT.WebServices.Content.Stats.Services;
using IT.WebServices.Content.Stats.Services.Data;
using IT.WebServices.Content.Stats.Services.Subscriptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddStatsClasses(this IServiceCollection services)
        {
            services.AddSingleton<ILikeDataProvider, FileSystemLikeDataProvider>();
            services.AddSingleton<IProgressDataProvider, FileSystemProgressDataProvider>();
            services.AddSingleton<ISaveDataProvider, FileSystemSaveDataProvider>();
            services.AddSingleton<IShareDataProvider, FileSystemShareDataProvider>();
            services.AddSingleton<IViewDataProvider, FileSystemViewDataProvider>();

            services.AddSingleton<IStatsContentPublicDataProvider, FileSystemStatsContentPublicDataProvider>();
            services.AddSingleton<IStatsContentPrivateDataProvider, FileSystemStatsContentPrivateDataProvider>();
            services.AddSingleton<IStatsUserPublicDataProvider, FileSystemStatsUserPublicDataProvider>();
            services.AddSingleton<IStatsUserPrivateDataProvider, FileSystemStatsUserPrivateDataProvider>();

            services.AddSingleton<SubscriptionList>();
            services.AddSingleton<ContentSubscription>();
            services.AddSingleton<UserSubscription>();

            return services;
        }

        public static void LoadStatsSubscriptions(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<ContentSubscription>().Load();
            app.ApplicationServices.GetService<UserSubscription>().Load();
        }

        public static void MapStatsGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<LikeService>();
            endpoints.MapGrpcService<ProgressService>();
            endpoints.MapGrpcService<QueryService>();
            endpoints.MapGrpcService<SaveService>();
            endpoints.MapGrpcService<ShareService>();
            endpoints.MapGrpcService<ViewService>();
        }
    }
}
