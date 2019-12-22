using System;
using System.Collections.Generic;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using safnet.Identity.Api.Infrastructure.MVC;
using safnet.Identity.Api.Infrastructure.Persistence;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using static IdentityModel.OidcConstants;

namespace safnet.Identity.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "safnet.Identity.Api";

            var logger = CreateLogger();
            try
            {
                var configuration = ReadAppSettings();
                CreateInitialKeyAndSecret(configuration);

                logger.Debug("Starting web application");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in application startup");
            }

            void CreateInitialKeyAndSecret(IConfigurationRoot configuration)
            {
                var initialClientKey = configuration.GetValue<string>(Constants.InitialClientKeyKey);
                var initialClientSecret = configuration.GetValue<string>(Constants.InitialClientSecretKey);

                if (!string.IsNullOrWhiteSpace(initialClientKey) && !string.IsNullOrWhiteSpace(initialClientSecret))
                {
                    logger.Information("Creating initial client key and secret");
                    var connectionString = configuration.GetConnectionString(Constants.IdentityConnectionStringName);

                    var dbContextOptions = new DbContextOptionsBuilder<IdentityContext>();
                    dbContextOptions.UseSqlServer(connectionString);
                    dbContextOptions.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    var clientRepo = new IdentityContext(dbContextOptions.Options) as IRepository<Client>;

                    _ = clientRepo.CreateAsync(new Client
                    {
                        ClientId = initialClientKey,
                        ClientName = initialClientKey,
                        AllowedGrantTypes = new List<ClientGrantType>
                            {new ClientGrantType {GrantType = GrantTypes.ClientCredentials}},
                        AllowedScopes = new List<ClientScope> {new ClientScope {Scope = StandardScopes.OpenId}},
                        ClientSecrets = new List<ClientSecret>
                            {new ClientSecret {Value = initialClientSecret.Sha256()}}
                    }).Result;
                }
            }

            IConfigurationRoot ReadAppSettings()
            {
                logger.Debug("Reading appsettings");
                var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                    .AddEnvironmentVariables();
                var configuration = builder.Build();
                return configuration;
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((context, configuration) => { ConfigureSerilog(configuration); });
        }

        private static ILogger CreateLogger()
        {
            return ConfigureSerilog()
                .CreateLogger();
        }

        private static LoggerConfiguration ConfigureSerilog(LoggerConfiguration configuration = null)
        {
            return (configuration ?? new LoggerConfiguration())
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(@"safnet.Identity.Api.log")
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                    theme: AnsiConsoleTheme.Literate);
        }
    }
}