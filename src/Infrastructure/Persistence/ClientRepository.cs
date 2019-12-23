using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AngleSharp;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using safnet.Common.GenericExtensions;

namespace safnet.Identity.Api.Infrastructure.Persistence
{
    public interface IGetByClientId<T>
    {
        Task<T> GetAsync(string clientId);
    }

    [ExcludeFromCodeCoverage]
    public class ClientRepository : IRepository<Client>, IGetByClientId<Client>
    {
        public static ClientRepository Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>()
                .UseSqlServer(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            var dbContext = new ConfigurationDbContext(optionsBuilder.Options, new ConfigurationStoreOptions());
            return new ClientRepository(dbContext);
        }

        //public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        //{
        //}

        private readonly ConfigurationDbContext _dbContext;
        public ClientRepository(ConfigurationDbContext dbContext)
        {
            _dbContext = dbContext.MustNotBeNull(nameof(dbContext));
        }
   

        public async Task<Client> CreateAsync(Client entity)
        {
            _dbContext.Clients.Add(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            _dbContext.Clients.Remove(new Client {Id = id});
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Client entity)
        {
            _dbContext.Clients.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Client>> GetAllAsync()
        {
            return await _dbContext.Clients.ToListAsync();
        }

        public async Task<Client> GetAsync(int id)
        {
            return await _dbContext.Clients.FirstOrDefaultAsync(x => x.Id == id);
        }
        
        public async Task<Client> UpdateAsync(Client entity)
        {
            _dbContext.Clients.Update(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }
        async Task<Client> IGetByClientId<Client>.GetAsync(string clientId)
        {
            return await _dbContext.Clients.FirstOrDefaultAsync(x => x.ClientId == clientId);
        }
    }
}