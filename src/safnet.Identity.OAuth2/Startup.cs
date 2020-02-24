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
using safnet.Common.Api.Middleware;
using safnet.Common.GenericExtensions;
using safnet.identity.common.Domain.Entities;
using safnet.identity.common.Infrastructure.Persistence;
using safnet.identity.common.Services.Adapters;
using Serilog;

namespace safnet.identity.OAuth2
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public const string IdentityConnectionStringName = "Identity";

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

            var connectionString = Configuration.GetConnectionString(IdentityConnectionStringName);

            ConfigureAspNetIdentity();
            ConfigureIdentityServer();
            ConfigureMvc();
            ConfigureLogging();
            ConfigureBearerAuth();
            ConfigureReact();
            ConfigureCors();

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
                    .AddIdentityServer(options =>
                    {
                        options.Events.RaiseErrorEvents = true;
                        options.Events.RaiseInformationEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseSuccessEvents = true;
                    })
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
                    .AddAspNetIdentity<ApplicationUser>();

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

                services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<UserDbContext>();

                services.AddTransient<IUserManager, UserManagerAdapter>();
            }

            void ConfigureCors()
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("default", policy =>
                    {
                        policy.WithOrigins("https://localhost:44309")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
                });
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
                //app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            loggerFactory.AddSerilog();

            app.UseHttpsRedirection()
                .UseMiddleware<ExceptionLoggingMiddleware>()
                .UseIdentityServer()
                //.UseAuthentication()
                .UseMvcWithDefaultRoute()
                .UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";

                    if (_env.IsDevelopment())
                    {
                        spa.UseReactDevelopmentServer(npmScript: "start");
                    }
                })
                ;
        }
    }
}