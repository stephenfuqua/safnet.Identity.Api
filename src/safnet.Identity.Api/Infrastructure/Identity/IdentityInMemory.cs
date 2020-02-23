//using System.Collections.Generic;
//using IdentityServer4.Models;

//namespace safnet.Identity.Api.Infrastructure.Identity
//{
//    public static class IdentityInMemory
//    {

//        public static List<IdentityResource> IdentityResources =>
//            new List<IdentityResource>
//            {
//                new IdentityResources.OpenId(),
//                new IdentityResources.Profile()
//            };

//        public static IEnumerable<ApiResource> Apis =>
//            new List<ApiResource>
//            {
//                new ApiResource("admin", "My API")
//            };

//        public static IEnumerable<Client> Clients =>
//            new List<Client>
//            {
//                new Client
//                {
//                    ClientId = "safnet",

//                    // no interactive user, use the clientid/secret for authentication
//                    AllowedGrantTypes = GrantTypes.ClientCredentials,

//                    // secret for authentication
//                    ClientSecrets =
//                    {
//                        new Secret("Dirigible1".Sha256())
//                    },

//                    // scopes that client has access to
//                    AllowedScopes = { "admin" }
//                }
//            };
//    }
//}
