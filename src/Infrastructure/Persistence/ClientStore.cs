using AutoMapper;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using System.Threading.Tasks;

namespace safnet.Identity.Api.Infrastructure.Persistence
{
    public class ClientStore : IClientStore
    {
        private readonly IMapper _mapper;
        private readonly IdentityContext _context;
        // TODO: add SeriLog
        //private readonly ILogger _logger;

        // TODO: change to IRepository
        public ClientStore(IdentityContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var entity = await _context.Clients.FindAsync(clientId);

            return _mapper.Map<Client>(entity);
        }
    }
}
