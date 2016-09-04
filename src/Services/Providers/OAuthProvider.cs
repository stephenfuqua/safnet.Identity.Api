using FlightNode.Identity.Infrastructure.Persistence;
using log4net;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Threading.Tasks;

namespace FlightNode.Identity.Services.Providers
{
    public class OAuthProvider : OAuthAuthorizationServerProvider
    {
        private ILog _logger;

        public ILog Logger
        {
            get
            {
                return _logger ?? (_logger = LogManager.GetLogger(GetType().FullName));
            }
            set
            {
                _logger = value;
            }
        }


        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            try {

                var userManager = context.OwinContext.GetUserManager<AppUserManager>();

                var user = await userManager.FindAsync(context.UserName, context.Password);

                if (user == null || user.LockoutEnabled)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect, or the account is locked out.");
                    return;
                }

                var oAuthIdentity = await user.GenerateUserIdentityAsync(userManager, "JWT");

                var ticket = new AuthenticationTicket(oAuthIdentity, null);

                context.Validated(ticket);
            }
            catch(Exception ex)
            {
                Logger.Error(ex);
                context.SetError("Internal server error");
            }

        }
    }
}
