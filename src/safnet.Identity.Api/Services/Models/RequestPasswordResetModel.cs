using System.ComponentModel.DataAnnotations;

namespace FlightNode.Identity.Services.Models
{
    public class RequestPasswordResetModel
    {
        [Required]
        public string EmailAddress { get; set; }
    }
}
