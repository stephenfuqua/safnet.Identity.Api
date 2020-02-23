using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightNode.Identity.Services.Models
{
    /// <summary>
    /// Data-transfer object representing a system user
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// E-mail Address
        /// </summary>
        [RegularExpression(".+@.+\\..+")] // getting e-mails right is hard. Just ensure has some text at symbol a domain and a TLD.
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// Primary Phone Number
        /// </summary>
        [Phone]
        [Required]
        [StringLength(256)]
        public string PrimaryPhoneNumber { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        [Required]
        [StringLength(256)]
        public string UserName { get; set; }

        /// <summary>
        /// Secondary Phone Number
        /// </summary>
        [Phone]
        [StringLength(256)]
        public string SecondaryPhoneNumber { get; set; }

        /// <summary>
        /// Given ("first") Name
        /// </summary>
        [Required]
        [StringLength(50)]
        public string GivenName { get; set; }

        /// <summary>
        /// Family ("last") Name
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FamilyName { get; set; }

        /// <summary>
        /// Password. Only used for initial creation. Ignored in PUT/update
        /// </summary>
        [DataType(DataType.Password)]
        [StringLength(256)]
        public string Password { get; set; }

        /// <summary>
        /// When true, the user is locked out of the system.
        /// </summary>
        public bool LockedOut { get; set; }
        
        /// <summary>
        /// User's role.
        /// </summary>
        public int Role { get; set; }

        /// <summary>
        /// Returns concatenated GivenName and FamilyName.
        /// </summary>
        public string DisplayName {  get { return GivenName + " " + FamilyName;  } }

        /// <summary>
        /// Gets or sets the active status, typically one of { active, inactive, pending }.
        /// </summary>
        public bool Active { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string MailingAddress { get; set; }
        public string County { get; set; }

        
    }
}
