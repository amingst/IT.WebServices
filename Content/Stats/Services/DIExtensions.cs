using IT.WebServices.Content.Stats;
using IT.WebServices.Content.Stats.Services;
using IT.WebServices.Content.Stats.Services.Data;
using IT.WebServices.Content.Stats.Services.Data.FileSystem;
using IT.WebServices.Content.Stats.Services.Data.Sql;
using IT.WebServices.Content.Stats.Services.Subscriptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddStatsClasses(this IServiceCollection services)
        {
            services.AddSingleton<ILikeDataProvider, SqlLikeDataProvider>();
            services.AddSingleton<IProgressDataProvider, SqlProgressDataProvider>();
            services.AddSingleton<ISaveDataProvider, SqlSaveDataProvider>();
            services.AddSingleton<IShareDataProvider, SqlShareDataProvider>();
            services.AddSingleton<IViewDataProvider, SqlViewDataProvider>();

            services.AddSingleton<IStatsContentPublicDataProvider, SqlStatsContentQueryDataProvider>();
            services.AddSingleton<IStatsContentPrivateDataProvider, SqlStatsContentQueryDataProvider>();
            services.AddSingleton<IStatsUserPublicDataProvider, SqlStatsContentQueryDataProvider>();
            services.AddSingleton<IStatsUserPrivateDataProvider, SqlStatsContentQueryDataProvider>();

            services.AddScoped<IViewService, ViewService>();

            //services.AddSingleton<SubscriptionList>();
            //services.AddSingleton<ContentSubscription>();
            //services.AddSingleton<UserSubscription>();

            return services;
        }

        public static void LoadStatsSubscriptions(this IApplicationBuilder app)
        {
            //app.ApplicationServices.GetService<ContentSubscription>().Load();
            //app.ApplicationServices.GetService<UserSubscription>().Load();
        }

        public static void MapStatsGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<LikeService>();
            endpoints.MapGrpcService<ProgressService>();
            //endpoints.MapGrpcService<QueryFileService>();
            endpoints.MapGrpcService<QuerySqlService>();
            endpoints.MapGrpcService<SaveService>();
            endpoints.MapGrpcService<ShareService>();
            endpoints.MapGrpcService<ViewService>();
        }
    }
}
