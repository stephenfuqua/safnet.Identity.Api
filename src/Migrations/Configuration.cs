using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Infrastructure.Persistence;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Text;
using System.Linq;

namespace FlightNode.Identity.Migrations
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal sealed class Configuration : DbMigrationsConfiguration<FlightNode.Identity.Infrastructure.Persistence.IdentityDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(IdentityDbContext context)
        {
            // Initial Roles
            context.Roles.AddOrUpdate(r => r.Name,
                new Role { Id = 1, Name = RoleEnum.Administrator.ToString(), Description = "Administrative user" },
                new Role { Id = 2, Name = RoleEnum.Reporter.ToString(), Description = "Volunteer data reporter" },
                new Role { Id = 3, Name = RoleEnum.Coordinator.ToString(), Description = "Project coordinator" },
                new Role { Id = 4, Name = RoleEnum.Lead.ToString(), Description = "Volunteer team lead" }
            );
            SaveChanges(context);

            // Initial users

            if (!context.Users.Any(x => x.UserName == "asdf"))
            {
                var manager = new AppUserManager(new AppUserStore(context));

                var hashed = manager.PasswordHasher.HashPassword("dirigible1");
                var user = new User
                {
                    UserName = "asdf",
                    Email = "ab@asfddsdfs.com",
                    GivenName = "Juana",
                    FamilyName = "Coneja",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                    PasswordHash = hashed,
                    County = "a",
                    City = "b",
                    State = "c",
                    ZipCode = "d",
                    PhoneNumber = "e",
                    MobilePhoneNumber = "f",
                    Active = "active",
                    SecurityStamp = System.Guid.NewGuid().ToString()
                };

                context.Users.Add(user);
                SaveChanges(context);

                manager.AddToRolesAsync(user.Id, new[] {"Administrator"}).Wait();
            }



        }

        private void SaveChanges(IdentityDbContext context)
        {
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                throw new DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" +
                    sb.ToString(), ex
                ); // Add the original exception as the innerException
            }
        }
    }
}
