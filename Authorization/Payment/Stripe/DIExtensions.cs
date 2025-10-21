﻿using IT.WebServices.Authorization.Payment.Stripe;
using IT.WebServices.Authorization.Payment.Stripe.Clients;
using IT.WebServices.Authorization.Payment.Stripe.Data;
using IT.WebServices.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        public static IServiceCollection AddStripeClasses(this IServiceCollection services)
        {
            services.AddPaymentBaseClasses();
            services.AddSettingsHelpers();

            services.AddSingleton<MySQLHelper>();

            services.AddSingleton<StripeClient>();
            services.AddSingleton<IProductRecordProvider, FileSystemProductRecordProvider>();

            return services;
        }

        public static void MapStripeGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<StripeService>();
        }
    }
}
