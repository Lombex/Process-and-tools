
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
        public IActionResult GetAll()
        {
            var warehouses = _warehouseService.GetAllWarehouses();
            return Ok(warehouses);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var warehouse = _warehouseService.GetWarehouseById(id);
            if (warehouse == null)
                return NotFound($"Warehouse with id {id} not found.");
            return Ok(warehouse);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Warehouse warehouse)
        {
            if (warehouse == null)
                return BadRequest("Warehouse is null.");

            _warehouseService.AddWarehouse(warehouse);
            return CreatedAtAction(nameof(Get), new { id = warehouse.id }, warehouse);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Warehouse warehouse)
        {
            if (warehouse == null)
                return BadRequest("Warehouse is null.");

            var updated = _warehouseService.UpdateWarehouse(id, warehouse);
            if (!updated)
                return NotFound($"Warehouse with id {id} not found.");

            return Ok("Warehouse has been updated"); // 204 No Content
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var deleted = _warehouseService.DeleteWarehouse(id);
            if (!deleted)
                return NotFound($"Warehouse with id {id} not found.");

            return Ok("Warehouse has been deleted"); // 204 No Content
        }
    }
}
