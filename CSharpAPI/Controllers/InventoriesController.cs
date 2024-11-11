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

        [HttpGet]
        public ActionResult<IEnumerable<InventoriesModel>> GetAllInventories()
        {
            return Ok(_inventoriesService.GetAllInventories());
        }

        [HttpGet("{id}")]
        public ActionResult<InventoriesModel> GetInventoryById(int id)
        {
            try
            {
                return Ok(_inventoriesService.GetInventoryById(id));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult<InventoriesModel> CreateInventory(InventoriesModel inventory)
        {
            _inventoriesService.CreateInventory(inventory);
            return CreatedAtAction(nameof(GetInventoryById), new { id = inventory.id }, inventory);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateInventory(int id, InventoriesModel inventory)
        {
            if (_inventoriesService.UpdateInventory(id, inventory))
                return NoContent();
            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteInventory(int id)
        {
            if (_inventoriesService.DeleteInventory(id))
                return NoContent();
            return NotFound();
        }

        [HttpGet("item/{itemId}")]
        public ActionResult<IEnumerable<InventoriesModel>> GetInventoriesByItemId(string itemId)
        {
            return Ok(_inventoriesService.GetInventoriesByItemId(itemId));
        }

        [HttpGet("location/{locationId}")]
        public ActionResult<IEnumerable<InventoriesModel>> GetInventoriesByLocation(int locationId)
        {
            return Ok(_inventoriesService.GetInventoriesByLocation(locationId));
        }
    }
}