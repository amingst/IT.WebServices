using IT.WebServices.Content.Comment.Services;
using IT.WebServices.Content.Comment.Services.Data;
using IT.WebServices.Content.Comment.Services.Helper;
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
        public static IServiceCollection AddCommentClasses(this IServiceCollection services)
        {
            services.AddSingleton<ICommentDataProvider, FileSystemCommentDataProvider>();
            services.AddScoped<UserDataHelper>();

            return services;
        }

        public static void MapCommentGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<CommentService>();
        }
    }
}
