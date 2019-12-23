using AutoMapper;
using Models=IdentityServer4.Models;
using Entities=IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Stores;
using System.Threading.Tasks;
using safnet.Common.GenericExtensions;

namespace safnet.Identity.Api.Infrastructure.Persistence
{
    public class ClientStore : IClientStore
    {
        private readonly IMapper _mapper;
        private readonly IGetByClientId<Entities.Client> _repository;
        
        public ClientStore(IGetByClientId<Entities.Client> repository, IMapper mapper)
        {
            _repository = repository.MustNotBeNull(nameof(repository));
            _mapper = mapper.MustNotBeNull(nameof(mapper));
        }

        public async Task<Models.Client> FindClientByIdAsync(string clientId)
        {
            var entity = await _repository.GetAsync(clientId);

            return _mapper.Map<Models.Client>(entity);
        }
    }
}
