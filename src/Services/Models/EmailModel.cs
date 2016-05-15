using FlightNode.Common.Utility;
using System.ComponentModel.DataAnnotations;

namespace FlightNode.Identity.Services.Models
{
    public class EmailModel
    {
        [StringLength(100)]
        [Required]
        public string Subject { get; set; }

        [StringLength(100)]
        [Required]
        public string FromName { get; set; }

        [StringLength(250)]
        [Required]
        public string FromAddress { get; set; }

        [StringLength(1000)]
        [Required]
        public string Body { get; set; }

        /// <summary>
        /// Removes HTML from all fields
        /// </summary>
        /// <param name="sanitizer">An instance of <see cref="ISanitizer"/>.</param>
        public void Sanitize(ISanitizer sanitizer)
        {
            Subject = sanitizer.RemoveAllHtml(Subject);
            FromName = sanitizer.RemoveAllHtml(FromName);
            FromAddress = sanitizer.RemoveAllHtml(FromAddress);
            Body = sanitizer.RemoveAllHtml(Body);
        }
    }
}
