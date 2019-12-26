using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.EntityFramework.Options;
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

        public async Task<Client> CreateAsync(Client model)
        {
            model.MustNotBeNull(nameof(model));

            _dbContext.Clients.Add(model.ToEntity());
            await _dbContext.SaveChangesAsync();

            // No changes to map back to the model in this case

            return model;
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException("Deleting client by integer id is not supported");
        }

        public async Task DeleteAsync(Client model)
        {
            model.MustNotBeNull(nameof(model));

            _dbContext.Clients.Remove(model.ToEntity());

            await _dbContext.SaveChangesAsync();
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

        public async Task<Client> UpdateAsync(Client model)
        {
            model.MustNotBeNull(nameof(model));

            _dbContext.Clients.Update(model.ToEntity());
            await _dbContext.SaveChangesAsync();

            // No changes to map back to the model in this case

            return model;
        }

        public async Task<Client> GetByClientIdAsync(string clientId)
        {
            return (await _dbContext.Clients
                    .FirstOrDefaultAsync(x => x.ClientId == clientId)
                )?.ToModel();
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
    }
}