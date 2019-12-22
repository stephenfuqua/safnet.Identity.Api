using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace safnet.Identity.Api.Infrastructure.Persistence
{
    public interface IRepository<T>
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> GetAsync(int id);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task DeleteAsync(T entity);
    }

    [ExcludeFromCodeCoverage]
    public class IdentityContext : DbContext, IRepository<Client>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ClientGrantType> ClientGrantTypes { get; set; }
        public DbSet<ClientScope> ClientScopes { get; set; }

        public DbSet<ClientSecret> ClientSecrets { get; set; }

        async Task<Client> IRepository<Client>.CreateAsync(Client entity)
        {
            Clients.Add(entity);
            await SaveChangesAsync();

            return entity;
        }

        async Task IRepository<Client>.DeleteAsync(int id)
        {
            Clients.Remove(new Client {Id = id});
            await SaveChangesAsync();
        }

        async Task IRepository<Client>.DeleteAsync(Client entity)
        {
            Clients.Remove(entity);
            await SaveChangesAsync();
        }

        async Task<IReadOnlyList<Client>> IRepository<Client>.GetAllAsync()
        {
            return await Clients.ToListAsync();
        }

        async Task<Client> IRepository<Client>.GetAsync(int id)
        {
            return await Clients.FirstOrDefaultAsync(x => x.Id == id);
        }

        async Task<Client> IRepository<Client>.UpdateAsync(Client entity)
        {
            Clients.Update(entity);
            await SaveChangesAsync();

            return entity;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Client>().HasKey(m => m.Id);
            builder.Entity<ApiResource>().HasKey(m => m.Id);
            builder.Entity<IdentityResource>().HasKey(m => m.Id);
            base.OnModelCreating(builder);
        }
    }
}