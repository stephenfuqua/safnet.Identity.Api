using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using safnet.Common.GenericExtensions;

namespace safnet.Identity.Api.Infrastructure.Persistence
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client> GetByClientIdAsync(string clientId);
    }


    public class ClientRepository : IClientRepository
    {
        private readonly IConfigurationDbContext _dbContext;

        public ClientRepository(IConfigurationDbContext dbContext)
        {
            _dbContext = dbContext.MustNotBeNull(nameof(dbContext));
        }

        public async Task<int> CreateAsync(Client model)
        {
            model.MustNotBeNull(nameof(model));

            _dbContext.Clients.Add(model.ToEntity());
            return await _dbContext.SaveChangesAsync();
        }

        public Task<int> DeleteAsync(int id)
        {
            throw new NotImplementedException("Deleting client by integer id is not supported");
        }

        public async Task<int> DeleteAsync(Client model)
        {
            model.MustNotBeNull(nameof(model));

            var original = await GetByClientId(model.ClientId);
            if (original == null)
            {
                return 0;
            }
            _dbContext.Clients.Remove(original);

            return await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Client>> GetAllAsync()
        {
            return await _dbContext.Clients
                .Select(x => x.ToModel())
                .ToListAsync();
        }

        public Task<Client> GetAsync(int id)
        {
            throw new NotImplementedException("Querying for client by integer id is not supported");
        }

        public async Task<int> UpdateAsync(Client model)
        {
            model.MustNotBeNull(nameof(model));

            var original = await GetByClientId(model.ClientId);
            if (original == null)
            {
                return 0;
            }

            // TODO: map other values
            original.ClientName = model.ClientName;

            _dbContext.Clients.Update(original);

            return await _dbContext.SaveChangesAsync();
        }

        public async Task<Client> GetByClientIdAsync(string clientId)
        {
            return (await GetByClientId(clientId))?.ToModel();
        }

        [ExcludeFromCodeCoverage]
        public static ClientRepository Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>()
                .UseSqlServer(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            var dbContext = new ConfigurationDbContext(optionsBuilder.Options, new ConfigurationStoreOptions());
            return new ClientRepository(dbContext);
        }

        private async Task<IdentityServer4.EntityFramework.Entities.Client> GetByClientId(string clientId)
        {
            return await _dbContext.Clients
                .FirstOrDefaultAsync(x => x.ClientId == clientId);
        }
    }
}