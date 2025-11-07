using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Helpers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddShopsBaseClasses(this IServiceCollection services)
        {
            services.AddSingleton<MySQLHelper>();
            services.AddSingleton<SettingsHelper>();
            return services;
        }
    }
}
