using IT.WebServices.Content.Stats.Services;
using IT.WebServices.Content.Stats.Services.Data;
using IT.WebServices.Content.Stats.Services.Helper;
using IT.WebServices.Content.Stats.Services.Subscriptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddStatsClasses(this IServiceCollection services)
        {
            //services.AddSingleton<ILikeDataProvider, EventDdLikeDataProvider>();
            //services.AddSingleton<IProgressDataProvider, EventDdProgressDataProvider>();
            //services.AddSingleton<ISaveDataProvider, EventDdSaveDataProvider>();
            //services.AddSingleton<IShareDataProvider, EventDdShareDataProvider>();
            //services.AddSingleton<IViewDataProvider, EventDdViewDataProvider>();

            services.AddSingleton<IStatsContentPublicDataProvider, FileSystemStatsContentPublicDataProvider>();
            services.AddSingleton<IStatsContentPrivateDataProvider, FileSystemStatsContentPrivateDataProvider>();
            services.AddSingleton<IStatsUserPublicDataProvider, FileSystemStatsUserPublicDataProvider>();
            services.AddSingleton<IStatsUserPrivateDataProvider, FileSystemStatsUserPrivateDataProvider>();

            //services.AddSingleton<ContentSubscription>();
            //services.AddSingleton<UserSubscription>();

            return services;
        }

        public static void MapStatsGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            //endpoints.MapGrpcService<LikeService>();
            //endpoints.MapGrpcService<ProgressService>();
            //endpoints.MapGrpcService<QueryService>();
            //endpoints.MapGrpcService<SaveService>();
            //endpoints.MapGrpcService<ShareService>();
            //endpoints.MapGrpcService<ViewService>();
        }
    }
}
