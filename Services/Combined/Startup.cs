﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IT.WebServices.Authentication;
using IT.WebServices.Settings;
using IT.WebServices.Services.Combined.Models;
using AuthS = IT.WebServices.Authentication.Services;
using CmsS = IT.WebServices.Content.CMS.Services;
using System;
using Microsoft.IdentityModel.Tokens;

namespace IT.WebServices.Services.Combined
{
    public class Startup
    {
        private static byte[] PONG_RESPONSE = { (byte)'p', (byte)'o', (byte)'n', (byte)'g' };

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                    //.AddApplicationPart(typeof(UserApiController).GetTypeInfo().Assembly)
                    //.AddApplicationPart(typeof(AssetApiController).GetTypeInfo().Assembly)
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.PropertyNamingPolicy = new NeutralNamingPolicy();
                    });
            ;

            //services.AddGrpc();
            //services.AddGrpcHttpApi();
            //services.AddGrpcReflection();

            services.AddSwaggerGen();
            services.AddGrpcSwagger();

            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddJwtAuthentication();
            services.AddSettingsHelpers();

            services.AddAuthenticationClasses();
            services.AddCMSClasses();
            services.AddCommentClasses();
            services.AddSettingsClasses();
            services.AddStatsClasses();

            CryptoProviderFactory.DefaultCacheSignatureProviders = false;

            Console.WriteLine("*** Loading pubkey: (" + JwtExtensions.GetPublicKey() + ")  ***");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Map("/ping", (app1) => app1.Run(async context =>
            {
                await context.Response.BodyWriter.WriteAsync(PONG_RESPONSE);
            }));

            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI();

            if (env.IsDevelopment())
                Program.IsDevelopment = true;

            app.UseRouting();

            app.UseJwtApiAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAuthenticationGrpcServices();
                endpoints.MapCMSGrpcServices();
                endpoints.MapCommentGrpcServices();
                endpoints.MapSettingsGrpcServices();
                endpoints.MapStatsGrpcServices();

                //endpoints.MapGrpcReflectionService();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}