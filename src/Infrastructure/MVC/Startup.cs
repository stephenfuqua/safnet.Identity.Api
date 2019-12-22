using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using safnet.Identity.Database;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using safnet.Identity.Api.Infrastructure.Persistence;
using IdentityServer4.Stores;
using IdentityServer4.Services;
using Models = IdentityServer4.Models;
using Entities = IdentityServer4.EntityFramework.Entities;

namespace safnet.Identity.Api.Infrastructure.MVC
{
    public class Startup
    {

        private IIdentityServerBuilder _identityServerBuilder;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureMvc();
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

            var connectionString = Configuration.GetConnectionString(Constants.IdentityConnectionStringName);

            ConfigureIdentityServer(connectionString);

            DbInstaller.Run(connectionString, Configuration);

            services.AddDbContext<IdentityContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
            
            services.AddScoped<IRepository<Entities.Client>, IdentityContext>();
            services.AddScoped<IClientStore, ClientStore>();


            services.AddSingleton<ICache<Models.Client>, DefaultCache<Models.Client>>();

            void ConfigureMvc()
            {
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            }

            void ConfigureIdentityServer(string conString)
            {
                // TODO: configure IsserUri in IdentityServer options
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
                    .AddClientStore<ClientStore>()
                    .AddClientStoreCache<CachingClientStore<ClientStore>>();
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                _identityServerBuilder.AddDeveloperSigningCredential();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                throw new InvalidOperationException("Need to setup a certificate for IdentityServer4");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }

            app.UseHttpsRedirection()
               .UseMvc()
               .UseIdentityServer()
               .UseHttpsRedirection();


        }
    }
}
