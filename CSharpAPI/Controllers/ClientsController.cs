using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Services;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsService _clientsService;

        public ClientsController(IClientsService clientsService)
        {
            _clientsService = clientsService;
        }

        [HttpGet("all")]
        public ActionResult<IEnumerable<ClientModel>> GetAllClients()
        {
            var clients = _clientsService.GetAllClients();
            return Ok(clients);
        }

        [HttpGet("{id}")]
        public ActionResult<ClientModel> GetClientById(int id)
        {
            var client = _clientsService.GetClientById(id);
            if (client == null)
            {
                return NotFound();
            }
            return Ok(client);
        }

        [HttpPost]
        public ActionResult<ClientModel> CreateClient([FromBody] ClientModel client)
        {
            _clientsService.CreateClient(client);
            return CreatedAtAction(nameof(GetClientById), new { id = client.id }, client);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateClient(int id, [FromBody] ClientModel client)
        {
            if (id != client.id)
            {
                return BadRequest();
            }

            var existingClient = _clientsService.GetClientById(id);
            if (existingClient == null)
            {
                return NotFound();
            }

            _clientsService.UpdateClient(client);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteClient(int id)
        {
            var client = _clientsService.GetClientById(id);
            if (client == null)
            {
                return NotFound();
            }

            _clientsService.DeleteClient(id);
            return NoContent();
        }
    }
}