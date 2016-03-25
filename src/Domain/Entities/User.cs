using FlightNode.Common.BaseClasses;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FlightNode.Identity.Domain.Entities
{
    public class User : IdentityUser<int, UserLogin, UserRole, UserClaim>, IEntity
    {
        public const string ToUserPattern = "{0} {1} <{2}>";
        public const string StatusActive = "active";
        public const string StatusPending = "pending";
        public const string StatusInactive = "inactive";

        // Exists so that other projects don't have to reference Identity framework.
        public override int Id
        {
            get { return base.Id;  }
            set { base.Id = value; }
        }

        public string Active { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string MobilePhoneNumber { get; set; }

        [Required]
        [StringLength(50)]  
        public string GivenName { get; set; }

        [Required]
        [StringLength(50)]
        public string FamilyName { get; set; }

        public string DisplayName {  get { return GivenName + " " + FamilyName; } }

        [Required]
        [StringLength(50)]
        public string County { get; set; }

        [StringLength(100)]
        public string MailingAddress { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(2)]
        public string State { get; set; }

        [StringLength(10)]
        public string ZipCode { get; set; }


        /// <summary>
        /// Creates a new <see cref="User"/>, initially in the inactive state.
        /// </summary>
        public User()
        {
            Active = StatusInactive;
        }

        /// <summary>
        /// Creates the identity object used for transmitting a token to a client.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="authenticationType"></param>
        /// <returns>User's claims</returns>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User, int> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in
            // CookieAuthenticationOptions.AuthenticationType 

            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            
            // Custom claims
            userIdentity.AddClaim(new Claim("displayName", this.DisplayName));

            return userIdentity;
        }


        public string FormattedEmail
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, ToUserPattern, GivenName, FamilyName, Email);
            }
        }
    }


}
