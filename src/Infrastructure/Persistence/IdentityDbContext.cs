using IdentityServer4.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace safnet.Identity.Api.Infrastructure.Persistence
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Client>().HasKey(m => m.Id);
            builder.Entity<ApiResource>().HasKey(m => m.Id);
            builder.Entity<IdentityResource>().HasKey(m => m.Id);
            base.OnModelCreating(builder);
        }
    }
}
