
using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Service;

namespace CSharpAPI.Controller
{
    [Route("api/v1/warehouses")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var warehouses = await _warehouseService.GetAllWarehouses();
            return Ok(warehouses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var warehouse = await _warehouseService.GetWarehouseById(id);
            if (warehouse == null) return NotFound($"Warehouse with id {id} not found.");
            return Ok(warehouse);
        }

        [HttpGet("{id}/location")]
        public async Task<IActionResult> LocationFromWarehouseID(int id)
        {
            var location = await _warehouseService.GetLocationFromWarehouseID(id);
            if (location == null) return NotFound($"Location with id {id} not found.");
            return Ok(location);
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WarehouseModel warehouse)
        {
            if (warehouse == null) return BadRequest("Warehouse is null.");
            await _warehouseService.AddWarehouse(warehouse);
            return CreatedAtAction(nameof(Get), new { id = warehouse.id }, warehouse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] WarehouseModel warehouse)
        {
            if (warehouse == null) return BadRequest(new { message = "Invalid warehouse data." });
            await _warehouseService.UpdateWarehouse(id, warehouse);
            return Ok("Warehouse has been updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _warehouseService.DeleteWarehouse(id);
            return Ok("Warehouse has been deleted");
        }
    }
}
