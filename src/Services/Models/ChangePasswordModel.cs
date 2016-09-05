using System.ComponentModel.DataAnnotations;

namespace FlightNode.Identity.Services.Models
{
    public class ChangePasswordModel
    {
        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
