using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Service;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/items")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemsService _itemsService;

        public ItemsController(IItemsService itemsService)
        {
            _itemsService = itemsService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllItems()
        {
            var _items = await _itemsService.GetAllItems();
            return Ok(_items);
        }

        [HttpGet("{uid}")]
        public async Task<IActionResult> GetItemById(string uid)
        {
            var _item = await _itemsService.GetItemById(uid);
            if (_item == null) return NotFound($"Item with uid {uid} not found.");
            return Ok(_item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(ItemModel item)
        {
            if (item == null) return BadRequest("Request is empty!");
            await _itemsService.CreateItem(item);
            return CreatedAtAction(nameof(GetItemById), new { uid = item.uid }, item);
        }

        [HttpPut("{uid}")]
        public async Task<IActionResult> UpdateItem(string uid, ItemModel item)
        {
            if (item == null) return BadRequest("Request is empty!");
            await _itemsService.UpdateItem(uid, item);
            return Ok($"Item {uid} has been updated!");
        }

        [HttpDelete("{uid}")]
        public async Task<IActionResult> DeleteItem(string uid)
        {
            await _itemsService.DeleteItem(uid);
            return Ok("Item has been deleted!");
        }

        [HttpGet("line/{lineId}")]
        public async Task<IActionResult> GetItemsByLineId(int lineId)
        {
            var _itemline = await _itemsService.GetItemsByLineId(lineId);
            if (_itemline == null) return NotFound("Itemline not found!");
            return Ok(_itemline);
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetItemsByGroupId(int groupId)
        {
            var _itemgroup = await _itemsService.GetItemsByGroupId(groupId);
            if (_itemgroup == null) return NotFound("Itemgroup not found!");
            return Ok(_itemgroup);
        }

        [HttpGet("supplier/{supplierId}")]
        public async Task<IActionResult> GetItemsBySupplierId(int supplierId)
        {
            var _supplierId = await _itemsService.GetItemsBySupplierId(supplierId);
            if (_supplierId == null) return NotFound("Supplier not found!");
            return Ok(_supplierId);
        }
    }

}