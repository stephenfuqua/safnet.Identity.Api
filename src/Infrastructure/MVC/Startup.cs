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

namespace safnet.Identity.Api.Infrastructure.MVC
{
    public class Startup
    {
        private const string IdentityConnectionStringName = "Identity";
        private const string InitialClientKeyKey = "InitialClientKey";
        private const string InitialClientSecretKey = "InitialClientSecret";

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

            var connectionString = Configuration.GetConnectionString(IdentityConnectionStringName);

            ConfigureIdentityServer(connectionString);

            DbInstaller.Run(connectionString, Configuration);

            services.AddDbContext<IdentityDbContext>(options =>
                  options.UseSqlServer(connectionString)
              );

            services.AddScoped<IClientCrudStore, ClientStore>();


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
                    .AddClientStore<ClientStore>();
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
               .UseIdentityServer();
    

            var initialClientKey = Configuration.GetValue<string>(InitialClientKeyKey);
            var initialClientSecret = Configuration.GetValue<string>(InitialClientSecretKey);

            //var clientStore = app.ApplicationServices.GetService<IClientCrudStore>();

            //var dbService = app.ApplicationServices.GetService<IdentityDbContext>();
            //var mapper = app.ApplicationServices.GetService
            //var clientStore = new ClientStore(dbService, new )
            
            //clientStore.AddAsync(initialClientKey, initialClientSecret).Wait();
        }
    }
}
