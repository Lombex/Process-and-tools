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

        [HttpGet]
        public ActionResult<IEnumerable<ItemModel>> GetAllItems()
        {
            return Ok(_itemsService.GetAllItems());
        }

        [HttpGet("{uid}")]
        public ActionResult<ItemModel> GetItemById(string uid)
        {
            try
            {
                return Ok(_itemsService.GetItemById(uid));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult<ItemModel> CreateItem(ItemModel item)
        {
            _itemsService.CreateItem(item);
            return CreatedAtAction(nameof(GetItemById), new { uid = item.uid }, item);
        }

        [HttpPut("{uid}")]
        public IActionResult UpdateItem(string uid, ItemModel item)
        {
            if (_itemsService.UpdateItem(uid, item))
                return NoContent();
            return NotFound();
        }

        [HttpDelete("{uid}")]
        public IActionResult DeleteItem(string uid)
        {
            if (_itemsService.DeleteItem(uid))
                return NoContent();
            return NotFound();
        }

        [HttpGet("line/{lineId}")]
        public ActionResult<IEnumerable<ItemModel>> GetItemsByLineId(int lineId)
        {
            return Ok(_itemsService.GetItemsByLineId(lineId));
        }

        [HttpGet("group/{groupId}")]
        public ActionResult<IEnumerable<ItemModel>> GetItemsByGroupId(int groupId)
        {
            return Ok(_itemsService.GetItemsByGroupId(groupId));
        }

        [HttpGet("supplier/{supplierId}")]
        public ActionResult<IEnumerable<ItemModel>> GetItemsBySupplierId(int supplierId)
        {
            return Ok(_itemsService.GetItemsBySupplierId(supplierId));
        }
    }

}