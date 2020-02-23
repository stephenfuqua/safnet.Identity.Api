using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using safnet.Identity.Api.Infrastructure.MVC;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace safnet.identity.OAuth2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "safnet.Identity.Api";

            Log.Logger = CreateLogger();
            try
            {
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
                    // TODO: control some of this via appSettings
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