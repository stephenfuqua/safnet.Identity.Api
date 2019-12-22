using AutoMapper;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using System.Linq;
using System.Threading.Tasks;
using Entities = IdentityServer4.EntityFramework.Entities;

namespace safnet.Identity.Api.Infrastructure.Persistence
{
    public interface IClientCrudStore : IClientStore
    {
        Task<Client> AddAsync(string clientKey, string clientSecret);
    }

    public class ClientStore : IClientCrudStore
    {
        private readonly IMapper _mapper;
        private readonly IdentityDbContext _context;
        // TODO: add SeriLog
        //private readonly ILogger _logger;

        public ClientStore(IdentityDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var entity = await _context.Clients.FindAsync(new { clientId });

            return  _mapper.Map<Client>(entity);
        }

        public async Task<Client> AddAsync(string clientKey, string clientSecret)
        {
            var client = new Entities.Client
            {
                ClientId = clientKey
            };
            client.ClientSecrets.Add(new Entities.ClientSecret { Value = clientSecret });

            await _context.Clients.AddAsync(client);

            return _mapper.Map<Client>(client);
        }

    }
}
