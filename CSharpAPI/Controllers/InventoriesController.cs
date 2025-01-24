using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/inventories")]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoriesService _inventoriesService;
        private readonly IAuthService _authService;

        public InventoriesController(IInventoriesService inventoriesService, IAuthService authService)
        {
            _inventoriesService = inventoriesService;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "inventories", method);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllInventories([FromQuery] int page)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var inventories = await _inventoriesService.GetAllInventories();

            int totalItem = inventories.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var Elements = inventories.Skip((page * 10)).Take(10).Select(x => new
            {
                ID = x.id,
                Item_id = x.item_id,
                Description = x.description,
                Item_reference = x.item_reference,
                Locations = x.locations,
                Total_on_hand = x.total_on_hand,
                Total_expected = x.total_expected,
                Total_ordered = x.total_ordered,
                Total_allocated = x.total_allocated,
                Total_available = x.total_available,
                Created_at = x.created_at,
                Updated_at = x.updated_at
            }).ToList().OrderBy(_ => _.ID);

            var Response = new
            {
                Page = page,
                PageSize = 10,
                TotalItems = totalItem,
                TotalPages = totalPages,
                Inventories = Elements
            };
            return Ok(Response);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<InventorieModel>> GetInventoryById(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

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
            if (!await CheckAccess("POST"))
                return Forbid();

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
            if (!await CheckAccess("PUT"))
                return Forbid();

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
            if (!await CheckAccess("DELETE"))
                return Forbid();

            var result = await _inventoriesService.DeleteInventory(id);
            if (result)
                return NoContent();

            return NotFound($"Inventory with id {id} not found.");
        }

        [HttpGet("item/{itemId}")]
        public async Task<ActionResult<IEnumerable<InventorieModel>>> GetInventoriesByItemId(string itemId)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var inventories = await _inventoriesService.GetInventoriesByItemId(itemId);
            return Ok(inventories);
        }

        [HttpGet("location/{locationId}")]
        public async Task<ActionResult<IEnumerable<InventorieModel>>> GetInventoriesByLocation(AmountPerLocation locationId)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var inventories = await _inventoriesService.GetInventoriesByLocation(locationId);
            return Ok(inventories);
        }
    }
}