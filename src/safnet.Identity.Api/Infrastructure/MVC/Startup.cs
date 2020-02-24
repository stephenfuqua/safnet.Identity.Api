using System.Diagnostics.CodeAnalysis;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using safnet.Common.Api.Middleware;
using safnet.Common.GenericExtensions;
using safnet.Identity.Api.Infrastructure.Persistence;
using safnet.identity.common.Domain.Entities;
using safnet.identity.common.Infrastructure.Persistence;
using safnet.identity.common.Services.Adapters;
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
            ConfigureServiceProviders();
            ConfigureMvc();
            ConfigureLogging();
            ConfigureBearerAuth();
            ConfigureCors();

            void ConfigureMvc()
            {
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                    .AddMvcCore()
                    .AddAuthorization()
                    .AddJsonFormatters();
            }

            void ConfigurePersistence()
            {
                // Using DbUp to install scripts, instead of EF Migrations
                DbInstaller.Run(connectionString, Configuration);

                services.AddDbContext<ConfigurationDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });

                services.AddDbContext<UserDbContext>(options =>
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

            void ConfigureServiceProviders()
            {
                services.AddTransient<IRepository<Client>, ClientRepository>();
                services.AddTransient<IClientRepository, ClientRepository>();

                services.AddSingleton(new ConfigurationStoreOptions());
                services.AddScoped<IConfigurationDbContext>(provider => provider.GetService<ConfigurationDbContext>());

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
                .UseCors("default")
                .UseAuthentication()
                .UseMvc();
        }
    }
}