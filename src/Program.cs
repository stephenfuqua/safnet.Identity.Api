using System;
using System.Collections.Generic;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
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

            Log.Logger = CreateLogger();
            try
            {
                CreateInitialKeyAndSecret();

                Log.Debug("Starting web application");
                CreateWebHostBuilder(Log.Logger)(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception in application startup");
            }
            finally
            {
                Log.CloseAndFlush();
            }

            void CreateInitialKeyAndSecret()
            {
                var configuration = ReadAppSettings();

                var initialClientKey = configuration.GetValue<string>(Constants.InitialClientKeyKey);
                var initialClientSecret = configuration.GetValue<string>(Constants.InitialClientSecretKey);

                if (!string.IsNullOrWhiteSpace(initialClientKey) && !string.IsNullOrWhiteSpace(initialClientSecret))
                {
                    Log.Information("Creating initial client key and secret");
                    var connectionString = configuration.GetConnectionString(Constants.IdentityConnectionStringName);

                    var clientRepo = IdentityContext.Create(connectionString) as IRepository<Client>;

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
                Log.Debug("Reading appsettings");
                var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                    .AddEnvironmentVariables();
                var configuration = builder.Build();
                return configuration;
            }

            ILogger CreateLogger()
            {
                return new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.File(@"safnet.Identity.Api.log")
                    .WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                        theme: AnsiConsoleTheme.Literate)
                    .CreateLogger();
            }

            Func<string[], IWebHostBuilder> CreateWebHostBuilder(ILogger logger)
            {
                return arguments =>
                    WebHost.CreateDefaultBuilder(arguments)
                        .UseStartup<Startup>()
                        .UseSerilog(logger);
            }
        }

    }
}