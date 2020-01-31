using System;
using System.Diagnostics.CodeAnalysis;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using safnet.Common.GenericExtensions;
using safnet.Identity.Api.Infrastructure.Persistence;
using safnet.Identity.Api.Services.Adapters;
using safnet.Identity.Database;
using Serilog;

namespace safnet.Identity.Api.Infrastructure.MVC
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration.MustNotBeNull(nameof(configuration));
            _env = env.MustNotBeNull(nameof(env));
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.MustNotBeNull(nameof(services));

            var connectionString = Configuration.GetConnectionString(Constants.IdentityConnectionStringName);

            ConfigurePersistence();
            ConfigureAspNetIdentity();
            ConfigureIdentityServer();
            ConfigureMvc();
            ConfigureLogging();
            ConfigureBearerAuth();
            ConfigureReact();

            void ConfigureMvc()
            {
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                    .AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            }

            void ConfigureReact()
            {
                services.AddSpaStaticFiles(configuration =>
                {
                    configuration.RootPath = "ClientApp/build";
                });
            }

            void ConfigureIdentityServer()
            {
                var identityServerBuilder = services
                    .AddIdentityServer()
                    .AddConfigurationStore(options =>
                    {
                        options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString);
                    })
                    .AddOperationalStore(options =>
                    {
                        options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString);
                        options.EnableTokenCleanup = true;
                        options.TokenCleanupInterval = 30;
                    })
                    .AddClientStoreCache<CachingClientStore<ClientStore>>()
                    .AddAspNetIdentity<IdentityUser>();

                if (_env.IsDevelopment())
                {
                    identityServerBuilder.AddDeveloperSigningCredential();
                }
                else
                {
                    throw new InvalidOperationException("Need to setup a certificate for IdentityServer4");
                }

                services.AddSingleton<ICache<Client>, DefaultCache<Client>>();

                services.AddDbContext<ConfigurationDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });
            }

            void ConfigurePersistence()
            {
                // Using DbUp to install scripts, instead of EF Migrations
                DbInstaller.Run(connectionString, Configuration);

                services.AddTransient<IRepository<Client>, ClientRepository>();
                services.AddTransient<IClientRepository, ClientRepository>();
            }

            void ConfigureLogging()
            {
                services.AddLogging(builder => builder.AddSerilog(dispose: true));
            }

            void ConfigureBearerAuth()
            {
                var jwtAuthorityUrl = Configuration.GetValue<string>("JwtAuthorityUrl");

                services.AddAuthentication("Bearer")
                    .AddJwtBearer("Bearer", options =>
                    {
                        options.Authority = jwtAuthorityUrl;
                        options.RequireHttpsMetadata = false;
                        options.Audience = "admin";
                    });
            }

            void ConfigureAspNetIdentity()
            {
                services.AddDbContext<UserDbContext>(builder =>
                    builder.UseSqlServer(connectionString));

                services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<UserDbContext>();

                services.AddTransient<IUserManager, UserManagerAdapter>();
            }
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.MustNotBeNull(nameof(app));
            loggerFactory.MustNotBeNull(nameof(loggerFactory));

            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            loggerFactory.AddSerilog();

            app.UseHttpsRedirection()
                .UseMiddleware<ExceptionLoggingMiddleware>()
                .UseIdentityServer()
                .UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller}/{action=Index}/{id?}");
                }).UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";

                    if (_env.IsDevelopment())
                    {
                        spa.UseReactDevelopmentServer(npmScript: "start");
                    }
                });
        }
    }
}