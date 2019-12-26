using System;
using System.IdentityModel.Tokens.Jwt;
using IdentityServer4.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using safnet.Common.GenericExtensions;
using safnet.Identity.Api.Infrastructure.Persistence;
using safnet.Identity.Database;
using Serilog;

namespace safnet.Identity.Api.Infrastructure.MVC
{
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

            ConfigurePersistence(connectionString);
            ConfigureIdentityServer(connectionString);
            ConfigureMvc();
            ConfigureLogging();
            ConfigureBearerAuth();

            void ConfigureMvc()
            {
                services
                    .AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            }

            void ConfigureIdentityServer(string conString)
            {
                var identityServerBuilder = services
                    .AddIdentityServer()
                    .AddDeveloperSigningCredential()
                    .AddConfigurationStore(options =>
                    {
                        options.ConfigureDbContext = builder => builder.UseSqlServer(conString);
                    })
                    .AddOperationalStore(options =>
                    {
                        options.ConfigureDbContext = builder => builder.UseSqlServer(conString);
                        options.EnableTokenCleanup = true;
                        options.TokenCleanupInterval = 30;
                    })
                    .AddClientStoreCache<CachingClientStore<ClientStore>>();

                if (_env.IsDevelopment())
                {
                    identityServerBuilder.AddDeveloperSigningCredential();
                }
                else
                {
                    throw new InvalidOperationException("Need to setup a certificate for IdentityServer4");
                }

                services.AddSingleton<ICache<Client>, DefaultCache<Client>>();
            }

            void ConfigurePersistence(string conString)
            {
                DbInstaller.Run(conString, Configuration);

                services.AddDbContext<ConfigurationDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });

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
                        options.Authority = jwtAuthorityUrl; //"https://localhost:44373";
                        options.RequireHttpsMetadata = false;
                        options.Audience = "admin";
                    });
            }
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.MustNotBeNull(nameof(app));
            loggerFactory.MustNotBeNull(nameof(loggerFactory));

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            // app.UseHsts();

            loggerFactory.AddSerilog();

            app.UseHttpsRedirection()
                .UseMiddleware<ExceptionLoggingMiddleware>()
                .UseIdentityServer()
                .UseMvc(routes => { 
                    //routes.MapRoute("DefaultWebPage", "{controller=Home}/{action=Index}/{id?}");
                    //routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
                });
        }
    }
}