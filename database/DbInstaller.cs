using System.Collections.Generic;
using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;
using safnet.Common.StringExtensions;

namespace safnet.Identity.Database
{
    public static class DbInstaller
    {
        private const string InitialClientKeyKey = "InitialClientKey";
        private const string InitialClientSecretKey = "InitialClientSecret";

        public static void Run(string connectionString, IConfiguration config)
        {
            connectionString.MustNotBeNullOrEmpty(nameof(connectionString));

            var initialClientKey = config.GetValue<string>(InitialClientKeyKey);
            var initialClientSecret = config.GetValue<string>(InitialClientSecretKey);

            var upgrader = DeployChanges.To
                           .SqlDatabase(connectionString)
                           .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                           .WithVariables(
                                new Dictionary<string, string>() {
                                    { InitialClientKeyKey, initialClientKey },
                                    { InitialClientSecretKey, initialClientSecret }
                               }
                            )
                           .LogToConsole()
                           .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw new DatabaseInstallFailureException("Unable to install the Identity database", result.Error);
            }
        }
    }
}
