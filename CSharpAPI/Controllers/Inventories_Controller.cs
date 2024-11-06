using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Service;

namespace CSharpAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoriesService _inventoriesService;

        public InventoriesController(IInventoriesService inventoriesService)
        {
            _inventoriesService = inventoriesService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<InventoriesModel>> GetAll()
        {
            return Ok(_inventoriesService.GetAllInventories());
        }

        [HttpGet("{id}")]
        public ActionResult<InventoriesModel> GetById(int id)
        {
            var inventory = _inventoriesService.GetInventoryById(id);
            if (inventory == null)
            {
                return NotFound();
            }
            return Ok(inventory);
        }

        [HttpPost]
        public ActionResult<InventoriesModel> Create(InventoriesModel inventory)
        {
            _inventoriesService.AddInventory(inventory);
            return CreatedAtAction(nameof(GetById), new { id = inventory.id }, inventory);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, InventoriesModel inventory)
        {
            if (id != inventory.id)
            {
                return BadRequest();
            }

            var existingInventory = _inventoriesService.GetInventoryById(id);
            if (existingInventory == null)
            {
                return NotFound();
            }

            _inventoriesService.UpdateInventory(id, inventory);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existingInventory = _inventoriesService.GetInventoryById(id);
            if (existingInventory == null)
            {
                return NotFound();
            }

            _inventoriesService.DeleteInventory(id);
            return NoContent();
        }
    }
}
