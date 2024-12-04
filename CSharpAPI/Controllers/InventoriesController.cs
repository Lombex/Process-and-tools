using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Service;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/inventories")]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoriesService _inventoriesService;

        public InventoriesController(IInventoriesService inventoriesService)
        {
            _inventoriesService = inventoriesService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<InventorieModel>>> GetAllInventories()
        {
            var inventories = await _inventoriesService.GetAllInventories();
            return Ok(inventories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InventorieModel>> GetInventoryById(int id)
        {
            try
            {
                var inventory = await _inventoriesService.GetInventoryById(id);
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<InventorieModel>> CreateInventory([FromBody] InventorieModel inventory)
        {
            if (inventory == null)
            {
                return BadRequest("Inventory data is null.");
            }

            await _inventoriesService.AddInventory(inventory);
            return CreatedAtAction(nameof(GetInventoryById), new { id = inventory.id }, inventory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] InventorieModel inventory)
        {
            if (inventory == null)
            {
                return BadRequest("Invalid inventory data.");
            }

            var result = await _inventoriesService.UpdateInventory(id, inventory);
            if (result)
                return NoContent();

            return NotFound($"Inventory with id {id} not found.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var result = await _inventoriesService.DeleteInventory(id);
            if (result)
                return NoContent();

            return NotFound($"Inventory with id {id} not found.");
        }

        [HttpGet("item/{itemId}")]
        public async Task<ActionResult<IEnumerable<InventorieModel>>> GetInventoriesByItemId(string itemId)
        {
            var inventories = await _inventoriesService.GetInventoriesByItemId(itemId);
            return Ok(inventories);
        }

        [HttpGet("location/{locationId}")]
        public async Task<ActionResult<IEnumerable<InventorieModel>>> GetInventoriesByLocation(int locationId)
        {
            var inventories = await _inventoriesService.GetInventoriesByLocation(locationId);
            return Ok(inventories);
        }
    }
}