using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using cloudscribe.SimpleContent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApp.Integration;
using cloudscribe.Web.Navigation.Caching;
using Microsoft.Extensions.Logging;
using Serilog;
using WebApp.RouteConstraints;
using Eo4Coding.Trade.AutoTrader.Runners;
using System.Threading;

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "GlobalResources");

            ConfigureAuthPolicy(services);


            // add users and settings from app-tenants-users.json
            services.Configure<MultiTenancyOptions>(Configuration.GetSection("MultiTenancy"));

            services.AddMultitenancy<SiteSettings, CachingSiteResolver>();
            services.AddScoped<cloudscribe.Web.SimpleAuth.Models.IUserLookupProvider, SiteUserLookupProvider>();
            services.AddScoped<cloudscribe.Web.SimpleAuth.Models.IAuthSettingsResolver, SiteAuthSettingsResolver>();
            services.AddCloudscribeSimpleAuth();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "application";
                options.DefaultChallengeScheme = "application";
            })
            .AddCookie("application", options =>
            {
                options.LoginPath = new PathString("/account/login");

            });

            services.AddRunners();

            services.AddSingleton<IOptionsMonitor<CookieAuthenticationOptions>, SiteCookieAuthenticationOptions>();

            services.AddScoped<cloudscribe.SimpleContent.Models.IProjectQueries, cloudscribe.SimpleContent.Storage.NoDb.ConfigProjectQueries>();
            services.AddNoDbStorageForSimpleContent();

            services.AddScoped<IPageNavigationCacheKeys, SiteNavigationCacheKeys>();
            services.AddScoped<ITreeCacheKeyResolver, SiteNavigationCacheKeyResolver>();
            services.AddCloudscribeNavigation(Configuration.GetSection("NavigationOptions"));
            //add content project settings from simplecontent-settings.json
            services.Configure<List<ProjectSettings>>(Configuration.GetSection("ContentProjects"));
            services.AddScoped<IProjectSettingsResolver, SiteProjectSettingsResolver>();

            // this implementation of IProjectSecurityResolver provides integration with SimpleAuth
            // to use a different authentication system you would implement and plugin your own IProjectSecurityResolver
            services.AddScoped<IProjectSecurityResolver, cloudscribe.SimpleContent.Security.SimpleAuth.ProjectSecurityResolver>();

            services.AddCloudscribeCommmon(Configuration);
            services.AddSimpleContentMvc();
            services.AddMetaWeblogForSimpleContent(Configuration.GetSection("MetaWeblogApiOptions"));
            services.AddSimpleContentRssSyndiction();
            services.AddCloudscribeFileManager(Configuration);
            services.AddSession();

            // Add MVC services to the services container.

            services.Configure<MvcOptions>(options =>
            {
                options.CacheProfiles.Add("SiteMapCacheProfile",
                     new CacheProfile
                     {
                         Duration = 30
                     });

                options.CacheProfiles.Add("RssCacheProfile",
                     new CacheProfile
                     {
                         Duration = 100
                     });

            });

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddMvc()
                .AddRazorOptions(options =>
                {
                    options.AddCloudscribeCommonEmbeddedViews();
                    options.AddCloudscribeNavigationBootstrap3Views();
                    options.AddEmbeddedViewsForSimpleAuth();
                    options.AddCloudscribeSimpleContentBootstrap3Views();
                    options.AddCloudscribeFileManagerBootstrap3Views();

                    options.ViewLocationExpanders.Add(new SiteViewLocationExpander());

                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory,
            IOptions<cloudscribe.Web.SimpleAuth.Models.SimpleAuthSettings> authSettingsAccessor,
            CollectionRunner runner,
            IApplicationLifetime applicationLifetime

            )
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug((category,loglever)=>category.ToLower().Contains("route"));

            Log.Logger = new LoggerConfiguration()
//.MinimumLevel.Debug()
//.WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "C:\\logs\\log-{Date}.txt"),
//                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] [{EventId}] {Message}{NewLine}{Exception}")
.ReadFrom.Configuration(Configuration)
.CreateLogger();
            loggerFactory.AddSerilog();



            app.Map("/RequestInfo", bldr => {
                bldr.Run(async context =>
                {
                    await context.Response.WriteAsync(
                        //$"< !doctype html >\r\n<html>< head></head><body>< style>span {{position: absolute;left: 150px}}</style >< p >< h1 > Request properties </ h1 >< ul >< li > Path :< span > {context.Request.Path} </ span ></ li >< li > Path Base:< span > {context.Request.PathBase} </ span ></ li >< li > Method :< span > {context.Request.Method} </ span ></ li >< li > query :< span > {context.Request.Query} </ span ></ li >< li > Host :< span > @Model.Request.Host.Value </ span ></ li >< li > Port :< span > @Model.Request.Host.Port </ span ></ li ></ ul > </ p ></body></html>");
                        $"Request properties \r\nPath :{context.Request.Path.Value}\r\nPath Base:{context.Request.PathBase} \r\nMethod :{context.Request.Method}\r\nQuery :{context.Request.QueryString}\r\nHost Full :{context.Request.Host.Value}\r\nHost :{context.Request.Host.Host}\r\nPort :{context.Request.Host.Port} ");

                });
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            // add inline demo middleware
            //app.Use(async (context, next) =>
            //{
            //    //context.Request.
            //    //await context.Response.WriteAsync("Hello from inline demo middleware...");
            //    // invoke the next middleware
            //    await next.Invoke();
            //});

            runner.CreateStatic(CancellationToken.None);

            app.UseStaticFiles();
            app.UseCloudscribeCommonStaticFiles();

            // custom 404 and error page - this preserves the status code (ie 404)
            app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
            app.UseSession();
            app.UseMultitenancy<SiteSettings>();

            app.UseAuthentication();
            
            app.UseMvc(routes =>
            {
                routes.AddSimpleContentStaticResourceRoutes();
                routes.AddCloudscribeFileManagerRoutes();
                //routes.AddStandardRoutesForSimpleContent();
                routes.AddRoutes();
                routes.MapRoute(
                    name: "login",
                    template: "pr-login/{action}",
                    defaults: new { controller = "Login",action="Index"});
                routes.MapRoute(
                    name: "def",
                    template: "{controller}/{action}",
                    defaults:new { controller = "Home", action = "Index" },
                    constraints:new { login = new DefaultLoginRouteConstraint("Login") }
                    );

                // this route is not really the default route unless you change the above
                //routes.MapRoute(
                //    name: "default",
                //    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }

        private void ConfigureAuthPolicy(IServiceCollection services)
        {
            //https://docs.asp.net/en/latest/security/authorization/policies.html

            services.AddAuthorization(options =>
            {
                // this policy currently means any user with a blogId claim can edit
                // would require somthing more for multi tenant blogs
                options.AddPolicy(
                    "BlogEditPolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Admins");
                    }
                 );

                options.AddPolicy(
                   "PageEditPolicy",
                   authBuilder =>
                   {
                       authBuilder.RequireRole("Admins");
                   });

                options.AddPolicy(
                    "FileManagerPolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Admins");
                    });

                options.AddPolicy(
                    "FileManagerDeletePolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Admins");
                    });

                // add other policies here 

            });

        }
    }
}