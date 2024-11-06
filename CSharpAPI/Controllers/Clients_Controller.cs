using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Services;

namespace CSharpAPI.Controllers
{
    [Route("api/v1/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsService _clientsService;

        public ClientsController(IClientsService clientsService)
        {
            _clientsService = clientsService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ClientsModel>> GetAllClients()
        {
            var clients = _clientsService.GetAllClients();
            return Ok(clients);
        }

        [HttpGet("{id}")]
        public ActionResult<ClientsModel> GetClientById(int id)
        {
            var client = _clientsService.GetClientById(id);
            if (client == null)
            {
                return NotFound();
            }
            return Ok(client);
        }

        [HttpGet("{id}/orders")]
        public ActionResult<IEnumerable<OrdersModel>> GetClientOrders(int id)
        {
            var client = _clientsService.GetClientById(id);
            if (client == null)
            {
                return NotFound();
            }

            var orders = _clientsService.GetClientOrders(id);
            return Ok(orders);
        }

        [HttpPost]
        public ActionResult<ClientsModel> CreateClient([FromBody] ClientsModel client)
        {
            _clientsService.CreateClient(client);
            return CreatedAtAction(nameof(GetClientById), new { id = client.id }, client);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateClient(int id, [FromBody] ClientsModel client)
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
