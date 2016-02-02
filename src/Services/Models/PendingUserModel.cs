

namespace FlightNode.Identity.Services.Models
{
    /// <summary>
    /// Models users whose registration is pending.
    /// </summary>
    public class PendingUserModel
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// E-mail Address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Primary Phone Number
        /// </summary>
        public string PrimaryPhoneNumber { get; set; }

        /// <summary>
        /// Secondary Phone Number
        /// </summary>
        public string SecondaryPhoneNumber { get; set; }

        /// <summary>
        /// Returns concatenated GivenName and FamilyName.
        /// </summary>
        public string DisplayName { get; set; }
    }
}
