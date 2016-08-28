using FlightNode.Common.Utility;
using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Infrastructure.Persistence;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Owin;
using System;
using System.Reflection;

namespace FlightNode.Identity.Services.Providers
{
    public static class ApiStartup
    {
        public static IAppBuilder Configure(IAppBuilder app)
        {
            app = ConfigureIdentityManagement(app);
            app = ConfigureOAuthTokenConsumption(app);

            return app;
        }

        private static IAppBuilder ConfigureIdentityManagement(IAppBuilder app)
        {
            app.CreatePerOwinContext(IdentityDbContext.Create);
            app.CreatePerOwinContext<AppUserManager>(AppUserManager.Create);
            
            var OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                //For Dev enviroment only (on production should be AllowInsecureHttpConnection = false)
                AllowInsecureHttp = Properties.Settings.Default.AllowInsecureHttpConnection,

                TokenEndpointPath = new PathString("/oauth/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new OAuthProvider(),
                ApplicationCanDisplayErrors = false,
                
                AccessTokenFormat = new JwtFormat(Properties.Settings.Default.IssuerUrl)
            };

            // OAuth 2.0 Bearer Access Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);

            return app;
        }

        private static IAppBuilder ConfigureOAuthTokenConsumption(IAppBuilder app)
        {
            var clientId = Properties.Settings.Default.ClientId;
            var clientSecret = TextEncodings.Base64Url.Decode(Properties.Settings.Default.ClientSecret);

            // Api controllers with an [Authorize] attribute will be validated with JWT
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    AllowedAudiences = new[] { clientId },
                    IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                    {
                        new SymmetricKeyIssuerSecurityTokenProvider(Properties.Settings.Default.IssuerUrl, clientSecret)
                    }
                });

            return app;
        }


        public static IUnityContainer ConfigureDependencyInjection(IUnityContainer container)
        {
            container = RegisterAllTypesIn(container, Assembly.GetExecutingAssembly());

            container.RegisterType<IdentityDbContext>();
            container.RegisterType(typeof(IUserStore<User, int>), typeof(AppUserStore));
            container.RegisterType<IEmailFactory, EmailFactory>();

            return container;
        }

        private static IUnityContainer RegisterAllTypesIn(IUnityContainer container, Assembly repoAssembly)
        {
            container.RegisterTypes(AllClasses.FromAssemblies(repoAssembly),
                                                 WithMappings.FromAllInterfacesInSameAssembly,
                                                 WithName.Default,
                                                 WithLifetime.PerResolve);

            return container;
        }

    }
}
