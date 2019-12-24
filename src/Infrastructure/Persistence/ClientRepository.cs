using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using safnet.Common.GenericExtensions;
using safnet.Identity.Api.Infrastructure.MVC;

namespace safnet.Identity.Api.Infrastructure.Persistence
{
    public class ClientRepository : IClientStore, IRepository<Client>
    {
        private readonly ConfigurationDbContext _dbContext;
        private readonly IMapper _mapper;

        public ClientRepository(ConfigurationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext.MustNotBeNull(nameof(dbContext));
            _mapper = mapper.MustNotBeNull(nameof(mapper));
        }

        public async Task<IdentityServer4.Models.Client> FindClientByIdAsync(string clientId)
        {
            var c = await _dbContext.Clients
                .Include(x => x.ClientSecrets)
                .Include(x => x.AllowedGrantTypes)
                .FirstOrDefaultAsync(x => x.ClientId == clientId);

            return _mapper.Map<IdentityServer4.Models.Client>(c);
        }

        public async Task<Client> CreateAsync(Client entity)
        {
            _dbContext.Clients.Add(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            _dbContext.Clients.Remove(new Client { Id = id });
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

        public static ClientRepository Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>()
                .UseSqlServer(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            var dbContext = new ConfigurationDbContext(optionsBuilder.Options, new ConfigurationStoreOptions());
            var mapper = new MapperConfiguration(x => x.AddProfile(typeof(AutoMapperProfile)))
                .CreateMapper();

            return new ClientRepository(dbContext, mapper);
        }
    }
}