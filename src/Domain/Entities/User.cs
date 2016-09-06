using FlightNode.Common.BaseClasses;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FlightNode.Identity.Domain.Entities
{
    /// <summary>
    /// Models a user in the application.
    /// </summary>
    public class User : IdentityUser<int, UserLogin, UserRole, UserClaim>, IEntity
    {
        /// <summary>
        /// Reg ex pattern for "First Last &lt;email&gt;"
        /// </summary>
        public const string ToUserPattern = "{0} {1} <{2}>";

        /// <summary>
        /// Active status
        /// </summary>
        public const string StatusActive = "active";

        /// <summary>
        /// Pending status
        /// </summary>
        public const string StatusPending = "pending";

        /// <summary>
        /// Inactive status
        /// </summary>
        public const string StatusInactive = "inactive";

        /// <summary>
        /// Gets or sets the User's identifier
        /// </summary>
        // Exists so that other projects don't have to reference Identity framework.
        public override int Id
        {
            get { return base.Id;  }
            set { base.Id = value; }
        }

        /// <summary>
        /// Gets or sets the active/pending/inactive status.
        /// </summary>
        public string Active { get; set; }

        /// <summary>
        /// Gets or sets the mobile phone number.
        /// </summary>
        [DataType(DataType.PhoneNumber)]
        public string MobilePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the given / first name.
        /// </summary>
        [Required]
        [StringLength(50)]  
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets the family / last name.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets the display name, as "GivenName FamilyName".
        /// </summary>
        public string DisplayName {  get { return GivenName + " " + FamilyName; } }

        /// <summary>
        /// Gets or sets the county of residence.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string County { get; set; }

        /// <summary>
        /// Gets or sets the mailing address.
        /// </summary>
        [StringLength(100)]
        public string MailingAddress { get; set; }

        /// <summary>
        /// Gets or sets the mailing city.
        /// </summary>
        [StringLength(50)]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the mailing state.
        /// </summary>
        [StringLength(2)]
        public string State { get; set; }


        /// <summary>
        /// Gets or sets the mailing zip code.
        /// </summary>
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

        /// <summary>
        /// Gets name and e-mail address together.
        /// </summary>
        public string FormattedEmail
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, ToUserPattern, GivenName, FamilyName, Email);
            }
        }
    }


}
