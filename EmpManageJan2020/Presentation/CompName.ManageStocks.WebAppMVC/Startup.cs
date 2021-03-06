namespace CompName.ManageStocks.WebAppMVC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.HttpsPolicy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using ElmahCore.Mvc;
    using CompName.ManageStocks.WebAppMVC.Infrastructure.CustomFilters;
    using Autofac;
    using CompName.ManageStocks.CrossCutting.Logging;
    using CompName.ManageStocks.IOC;
    using Castle.DynamicProxy;
    using Autofac.Extensions.DependencyInjection;
    using AutoMapper;
    using CompName.ManageStocks.WebAppMVC.Infrastructure.AutoMapper;
    using CompName.ManageStocks.CrossCutting.Configuration;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.CookiePolicy;
    using Microsoft.AspNetCore.ResponseCompression;
    using WebMarkupMin.AspNetCore3;
    using Newtonsoft.Json.Serialization;
    using CompName.ManageStocks.CrossCutting.InMemoryCaching;
    using StackExchange.Profiling.Storage;
    using Autofac.Extras.DynamicProxy;

    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public AppSetting AppSetting { get; set; } = new AppSetting();

        public IConfiguration Configuration { get; }

        public ILifetimeScope AutofacContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            this.Configuration.Bind("AppSetting", this.AppSetting);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                     .AddCookie(
                CookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                     {
                         options.LoginPath = "/Login";
                         options.AccessDeniedPath = "/AccessDenied";
                         options.SlidingExpiration = true;
                         options.Cookie.Name = this.AppSetting.AuthenticationSetting.AppCookieName + ".AuthKey";
                         options.Cookie.SameSite = SameSiteMode.Strict;
                     });

            services.AddAuthorization();

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes =
                    ResponseCompressionDefaults.MimeTypes.Concat(
                        new[] { "image/svg+xml" });
            });

            services.AddWebMarkupMin(options =>
            {
                options.AllowMinificationInDevelopmentEnvironment = true;
                options.AllowCompressionInDevelopmentEnvironment = true;
            })
                .AddHtmlMinification(options =>
                {
                })

               .AddXmlMinification()
               .AddHttpCompression()
               ;

            services.AddRazorPages().AddRazorRuntimeCompilation();

            services.AddElmah();

            services.AddControllersWithViews(options =>
        {
            options.Filters.Add(typeof(LoggingActionFilter));
        }).AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
        });

            services.AddAutoMapper(typeof(AutoMapperProfile));

            services.AddOptions();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterInstance(this.AppSetting).SingleInstance();
            builder.Register(c => new LogInterceptor(this.logger)).SingleInstance();
            builder.Register(c => new GlobalAppInMemoryCache(this.AppSetting.DatabaseSetting.SqlDbConnection))
                   .As<IGlobalAppInMemoryCache>().SingleInstance()
                   .EnableInterfaceInterceptors()
                   .InterceptedBy(typeof(LogInterceptor));

            builder.RegisterModule(new RepositoryIOCModule(this.AppSetting.DatabaseSetting.SqlDbConnection, "InstancePerLifetimeScope"));
            builder.RegisterModule(new ServiceIOCModule("InstancePerLifetimeScope"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            app.Use(
                next =>
                {
                    return async context =>
                    {
                        var stopWatch = new Stopwatch();
                        stopWatch.Start();
                        context.Response.OnStarting(
                            () =>
                            {
                                context.Response.Headers.Add("_ApplicationName", "CompName.ManageStocks.WebAppMVC");
                                context.Response.Headers.Add("_Developed-By", "CompName Org.");

                                context.Response.Headers.Add("RequestTime", DateTime.Now.ToString());
                                context.Response.Headers.Add("RequestId", context.TraceIdentifier);
                                stopWatch.Stop();

                                context.Response.Headers.Add("X-ResponseTime-Ms", stopWatch.ElapsedMilliseconds.ToString());
                                return Task.CompletedTask;
                            });

                        await next(context);
                    };
                });

            app.UseExceptionHandler("/Home/Error");

            app.UseHttpsRedirection();

            app.UseResponseCompression();
            app.UseWebMarkupMin();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseWhen(context => context.Request.Path.StartsWithSegments("/elmah", StringComparison.OrdinalIgnoreCase), appBuilder =>
            {
                appBuilder.Use(next =>
                {
                    return async ctx =>
                    {
                        ctx.Features.Get<IHttpBodyControlFeature>().AllowSynchronousIO = true;

                        await next(ctx);
                    };
                });
            });
            app.UseElmah();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = HttpOnlyPolicy.None,
                Secure = CookieSecurePolicy.None,
                MinimumSameSitePolicy = SameSiteMode.None,
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
                   name: "areas",
                   pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}