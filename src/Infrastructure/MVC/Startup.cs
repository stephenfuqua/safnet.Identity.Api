using System;
using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using safnet.Identity.Api.Infrastructure.Persistence;
using safnet.Identity.Database;
using Serilog;

namespace safnet.Identity.Api.Infrastructure.MVC
{
    public class Startup
    {
        private IIdentityServerBuilder _identityServerBuilder;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            var connectionString = Configuration.GetConnectionString(Constants.IdentityConnectionStringName);

            ConfigurePersistence(connectionString);
            ConfigureIdentityServer(connectionString);
            ConfigureMvc();

            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
            services.AddLogging(builder => builder.AddSerilog(dispose: true));


            void ConfigureMvc()
            {
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            }

            void ConfigureIdentityServer(string conString)
            {
                // TODO: configure IssuerUri in IdentityServer options
                _identityServerBuilder = services.AddIdentityServer()
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
                    //.AddClientStore<ClientStore>()
                    .AddClientStoreCache<CachingClientStore<ClientStore>>();

                services.AddSingleton<ICache<IdentityServer4.Models.Client>, DefaultCache<IdentityServer4.Models.Client>>();
                services.AddTransient<IClientStore, ClientStore>();
            }

            void ConfigurePersistence(string conString)
            {
                DbInstaller.Run(conString, Configuration);

                services.AddDbContext<IdentityContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });

                services.AddTransient<IRepository<Client>, IdentityContext>();
                services.AddTransient<IGetByClientId<Client>, IdentityContext>();
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                _identityServerBuilder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new InvalidOperationException("Need to setup a certificate for IdentityServer4");
            }

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            // app.UseHsts();

            loggerFactory.AddSerilog();

            app.UseHttpsRedirection()
                .UseMiddleware<ExceptionLoggingMiddleware>()
                .UseMvc()
                .UseIdentityServer();
        }
    }
}