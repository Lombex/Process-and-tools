using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Service;

namespace CSharpAPI.Controllers
{
    [Route("api/v1/items")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IItemsService _itemsService;

        public ItemsController(IItemsService itemsService)
        {
            _itemsService = itemsService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ItemsModel>> GetAllItems()
        {
            var items = _itemsService.GetAllItems();
            return Ok(items);
        }

        [HttpGet("{uid}")]
        public ActionResult<ItemsModel> GetItemById(string uid)
        {
            var item = _itemsService.GetItemById(uid);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        [HttpGet("items/itemline/{itemLineId}")]
        public ActionResult<IEnumerable<ItemsModel>> GetItemsByItemLine(int itemLineId)
        {
            var items = _itemsService.GetItemsByItemLine(itemLineId);
            return Ok(items);
        }

        [HttpGet("items/itemgroup/{itemGroupId}")]
        public ActionResult<IEnumerable<ItemsModel>> GetItemsByItemGroup(int itemGroupId)
        {
            var items = _itemsService.GetItemsByItemGroup(itemGroupId);
            return Ok(items);
        }

        [HttpGet("items/itemtype/{itemTypeId}")]
        public ActionResult<IEnumerable<ItemsModel>> GetItemsByItemType(int itemTypeId)
        {
            var items = _itemsService.GetItemsByItemType(itemTypeId);
            return Ok(items);
        }

        [HttpGet("items/supplier/{supplierId}")]
        public ActionResult<IEnumerable<ItemsModel>> GetItemsBySupplierId(int supplierId)
        {
            var items = _itemsService.GetItemsBySupplierId(supplierId);
            return Ok(items);
        }

        [HttpPut("{uid}")]
        public IActionResult UpdateItem(string uid, [FromBody] ItemsModel updatedItem)
        {
            if (uid != updatedItem.uid)
            {
                return BadRequest();
            }

            var existingItem = _itemsService.GetItemById(uid);
            if (existingItem == null)
            {
                return NotFound();
            }

            _itemsService.UpdateItem(updatedItem);
            return NoContent();
        }

        [HttpPost]
        public ActionResult<ItemsModel> CreateItem([FromBody] ItemsModel newItem)
        {
            _itemsService.AddItem(newItem);
            return CreatedAtAction(nameof(GetItemById), new { uid = newItem.uid }, newItem);
        }

        [HttpDelete("{uid}")]
        public IActionResult DeleteItem(string uid)
        {
            var existingItem = _itemsService.GetItemById(uid);
            if (existingItem == null)
            {
                return NotFound();
            }

            _itemsService.DeleteItem(uid);
            return NoContent();
        }
    }
}
