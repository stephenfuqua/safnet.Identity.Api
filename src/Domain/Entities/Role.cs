using FlightNode.Common.BaseClasses;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace FlightNode.Identity.Domain.Entities
{
    /// <summary>
    /// Model a user role in the application.
    /// </summary>
    public class Role : IdentityRole<int, UserRole>, IEntity
    {
        /// <summary>
        /// Gets or sets a description for the role.
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Description { get; set; }
    }
}
