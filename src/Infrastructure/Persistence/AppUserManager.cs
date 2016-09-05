using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Domain.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Threading.Tasks;

namespace FlightNode.Identity.Infrastructure.Persistence
{

    public class AppUserManager : UserManager<User, int>, IUserPersistence
    {
        public AppUserManager(IUserStore<User, int> store)
        : base(store)
        {
        }
        

        public static AppUserManager Create(IdentityFactoryOptions<AppUserManager> options, IOwinContext context)
        {
            var manager = new AppUserManager(new AppUserStore(context.Get<IdentityDbContext>()));

            manager = ConfigureUsernameValidation(manager);
            manager = ConfigurePasswordValidation(manager);
            manager = ConfigureLockout(manager);

            // Two-factor authenticadtion
            //manager.RegisterTwoFactorProvider("EmailCode",
            //    new EmailTokenProvider<User, int>
            //    {
            //        // TODO: subject should not be hard-coded to FlightNode. Need to be flexible
            //        // enough to handle the project name.
            //        Subject = "FlightNode Security Code",
            //        BodyFormat = "Your security code is: {0}"
            //    });

            //manager.EmailService = new EmailService();


            manager = ConfigureTokenProvider(options, manager);

            return manager;
        }

        private static AppUserManager ConfigureTokenProvider(IdentityFactoryOptions<AppUserManager> options, AppUserManager manager)
        {
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<User, int>(
                        dataProtectionProvider.Create("ASP.NET Identity"))
                    {
                        TokenLifespan = TimeSpan.FromHours(24)
                    };
            }
            return manager;
        }

        private static AppUserManager ConfigureLockout(AppUserManager manager)
        {
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;
            return manager;
        }

        private static AppUserManager ConfigurePasswordValidation(AppUserManager manager)
        {
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = false,
                RequireUppercase = false,
            };
            return manager;
        }

        private static AppUserManager ConfigureUsernameValidation(AppUserManager manager)
        {
            manager.UserValidator = new UserValidator<User, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            return manager;
        }
    }


    //public class EmailService : IIdentityMessageService
    //{
    //    public Task SendAsync(IdentityMessage message)
    //    {
    //        // TODO: Plug in your email service here to send an email.
    //        return Task.FromResult(0);
    //    }
    //}
}