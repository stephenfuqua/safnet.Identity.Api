using System;

namespace safnet.Identity.Database
{
    public class DatabaseInstallFailureException : Exception
    {
        public DatabaseInstallFailureException(string message) : base(message)
        {
        }

        public DatabaseInstallFailureException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }

}
