using System.Text;

namespace safnet.Identity.Api
{
    // TODO: consider moving to safnet.Common
    public static class StringExtensions
    {
        public static string aSha256(this string randomString)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new System.Text.StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
            foreach (var c in crypto)
            {
                hash.Append(c.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
