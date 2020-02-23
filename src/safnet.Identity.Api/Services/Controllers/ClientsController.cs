using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using safnet.Common.GenericExtensions;
using safnet.Identity.Api.Infrastructure.Persistence;
using safnet.Identity.Api.Services.Models;

namespace safnet.Identity.Api.Services.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        public const string ClientIdRouteName = "GetByClientId";

        public const string ErrorMessageClientAlreadyExists = "The provided client ID already exists";

        private readonly IClientRepository _clientRepository;

        public ClientsController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository.MustNotBeNull(nameof(clientRepository));
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var clients = await _clientRepository.GetAllAsync();

            return Ok(clients);
        }

        [HttpGet("{clientId:alpha}", Name = ClientIdRouteName)]
        public async Task<IActionResult> Get([FromRoute] string clientId)
        {
            var client = await _clientRepository.GetByClientIdAsync(clientId);

            if (client == null)
            {
                return NotFound();
            }

            return Ok(client);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Client model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _clientRepository.GetByClientIdAsync(model.ClientId);

            if (existing != null)
            {
                return Conflict(new MessageModel(ErrorMessageClientAlreadyExists));
            }

            await _clientRepository.CreateAsync(model);

            return CreatedAtRoute(ClientIdRouteName, new { clientId = model.ClientId }, model);
        }

        [HttpPut("{clientId:alpha}", Name = ClientIdRouteName)]
        public async Task<IActionResult> Put([FromRoute] string clientId, [FromBody] Client model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: validate clientId == model.ClientId

            var result = await _clientRepository.UpdateAsync(model);

            if (result == 0)
            {
                return NotFound();
            }

            return Accepted();
        }

        [HttpDelete("{clientId:alpha}", Name = ClientIdRouteName)]
        public async Task<IActionResult> Delete([FromRoute] string clientId)
        {
            var result = await _clientRepository.DeleteAsync(new Client { ClientId = clientId });

            if (result == 0)
            {
                return NotFound();
            }

            return Accepted();
        }
    }
}