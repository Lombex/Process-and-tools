using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services;
using CSharpAPI.Services.Auth;


namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsService _clientsService;
        private readonly IAuthService _authService;

        public ClientsController(IClientsService clientsService, IAuthService authService)
        {
            _clientsService = clientsService;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "clients", method);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ClientModel>>> GetAllClients()
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var clients = await _clientsService.GetAllClients();
            return Ok(clients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientModel>> GetClientById(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var client = await _clientsService.GetClientById(id);
            if (client == null)
            {
                return NotFound($"Client with id {id} not found.");
            }
            return Ok(client);
        }

        [HttpGet("{id}/orders")]
        public async Task<IActionResult> ClientOrders(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var _order = await _clientsService.GetClientOrders(id);
            if (_order == null) return NotFound($"Order with {id} not found");
            return Ok(_order);
        }

        [HttpPost]
        public async Task<ActionResult<ClientModel>> CreateClient([FromBody] ClientModel client)
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            if (client == null) return BadRequest("Client data is null.");

            await _clientsService.AddClient(client);
            return CreatedAtAction(nameof(GetClientById), new { id = client.id }, client);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] ClientModel client)
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

            if (client == null) return BadRequest("Invalid client data.");

            var existingClient = await _clientsService.GetClientById(id);
            if (existingClient == null)
            {
                return NotFound($"Client with id {id} not found.");
            }

            await _clientsService.UpdateClient(id, client);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            if (!await CheckAccess("DELETE"))
                return Forbid();

            var existingClient = await _clientsService.GetClientById(id);
            if (existingClient == null)
            {
                return NotFound($"Client with id {id} not found.");
            }

            await _clientsService.DeleteClient(id);
            return NoContent();
        }
    }
}