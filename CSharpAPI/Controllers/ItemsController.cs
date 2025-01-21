using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/items")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemsService _itemsService;
        private readonly IAuthService _authService;

        public ItemsController(IItemsService itemsService, IAuthService authService)
        {
            _itemsService = itemsService;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "items", method);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllItems([FromQuery] int page)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var _items = await _itemsService.GetAllItems();

            int totalItem = _items.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var Elements = _items.Skip((page * 10)).Take(10).Select(x => new
            {
                Uid = x.uid,
                Code = x.code,
                Description = x.description,
                Short_description = x.short_description,
                Upc_code = x.upc_code,
                Model_number = x.model_number,
                Commodity_code = x.commodity_code,
                Item_line = x.item_line,
                Item_group = x.item_group,
                Item_type = x.item_type,
                Unit_purchase_quantity = x.unit_purchase_quantity,
                Unit_order_quantity = x.unit_order_quantity,
                Pack_order_quantity = x.pack_order_quantity,
                Supplier_id = x.supplier_id,
                Supplier_code = x.supplier_code,
                Supplier_part_number = x.supplier_part_number,
                Created_at = x.created_at,
                Updated_at = x.updated_at
            }).ToList().OrderBy(_ => _.Uid);

            var Response = new
            {
                Page = page,
                PageSize = 10,
                TotalItems = totalItem,
                TotalPages = totalPages,
                Items = Elements
            };
            return Ok(Response);
        }

        [HttpGet("{uid}")]
        public async Task<IActionResult> GetItemById(string uid)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var _item = await _itemsService.GetItemById(uid);
            if (_item == null) return NotFound($"Item with uid {uid} not found.");
            return Ok(_item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(ItemModel item)
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            if (item == null) return BadRequest("Request is empty!");
            await _itemsService.CreateItem(item);
            return CreatedAtAction(nameof(GetItemById), new { uid = item.uid }, item);
        }

        [HttpPut("{uid}")]
        public async Task<IActionResult> UpdateItem(string uid, ItemModel item)
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

            if (item == null) return BadRequest("Request is empty!");
            await _itemsService.UpdateItem(uid, item);
            return Ok($"Item {uid} has been updated!");
        }

        [HttpDelete("{uid}")]
        public async Task<IActionResult> DeleteItem(string uid)
        {
            if (!await CheckAccess("DELETE"))
                return Forbid();

            await _itemsService.DeleteItem(uid);
            return Ok("Item has been deleted!");
        }

        [HttpGet("line/{lineId}")]
        public async Task<IActionResult> GetItemsByLineId(int lineId)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var _itemline = await _itemsService.GetItemsByLineId(lineId);
            if (_itemline == null) return NotFound("Itemline not found!");
            return Ok(_itemline);
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetItemsByGroupId(int groupId)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var _itemgroup = await _itemsService.GetItemsByGroupId(groupId);
            if (_itemgroup == null) return NotFound("Itemgroup not found!");
            return Ok(_itemgroup);
        }

        [HttpGet("supplier/{supplierId}")]
        public async Task<IActionResult> GetItemsBySupplierId(int supplierId)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var _supplierId = await _itemsService.GetItemsBySupplierId(supplierId);
            if (_supplierId == null) return NotFound("Supplier not found!");
            return Ok(_supplierId);
        }

        [HttpGet("type/{typeId}")]
        public async Task<IActionResult> GetItemsByTypeId(int typeId)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var _itemtype = await _itemsService.GetItemsByTypeId(typeId);
            if (_itemtype == null) return NotFound("Itemtype not found!");
            return Ok(_itemtype);
        }
    }
}