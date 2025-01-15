using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;

namespace CSharpAPI.Controller
{
    [Route("api/v1/warehouses")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IAuthService _authService;

        public WarehousesController(IWarehouseService warehouseService, IAuthService authService)
        {
            _warehouseService = warehouseService;
            _authService = authService;
        }

        private async Task<IActionResult> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            var hasAccess = await _authService.HasAccess(user, "warehouses", method);
            if (!hasAccess)
            {
                return StatusCode(403, new { message = "Access denied" });
            }
            return null;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var accessCheck = await CheckAccess("GET");
            if (accessCheck != null)
                return accessCheck;

            var warehouses = await _warehouseService.GetAllWarehouses();
            return Ok(warehouses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var accessCheck = await CheckAccess("GET");
            if (accessCheck != null)
                return accessCheck;

            var warehouse = await _warehouseService.GetWarehouseById(id);
            if (warehouse == null) return NotFound($"Warehouse with id {id} not found.");
            return Ok(warehouse);
        }

        [HttpGet("{id}/location")]
        public async Task<IActionResult> LocationFromWarehouseID(int id)
        {
            var accessCheck = await CheckAccess("GET");
            if (accessCheck != null)
                return accessCheck;

            var location = await _warehouseService.GetLocationFromWarehouseID(id);
            if (location == null) return NotFound($"Location with id {id} not found.");
            return Ok(location);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WarehouseModel warehouse)
        {
            var accessCheck = await CheckAccess("POST");
            if (accessCheck != null)
                return accessCheck;

            if (warehouse == null) return BadRequest("Warehouse is null.");
            await _warehouseService.AddWarehouse(warehouse);
            return CreatedAtAction(nameof(Get), new { id = warehouse.id }, warehouse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] WarehouseModel warehouse)
        {
            var accessCheck = await CheckAccess("PUT");
            if (accessCheck != null)
                return accessCheck;

            if (warehouse == null) return BadRequest(new { message = "Invalid warehouse data." });
            await _warehouseService.UpdateWarehouse(id, warehouse);
            return Ok("Warehouse has been updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var accessCheck = await CheckAccess("DELETE");
            if (accessCheck != null)
                return accessCheck;

            await _warehouseService.DeleteWarehouse(id);
            return Ok("Warehouse has been deleted");
        }
    }
}