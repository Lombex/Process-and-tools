using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/itemtypes")]
    public class ItemTypesController : ControllerBase
    {
        private readonly IItemTypeService _service;

        public ItemTypesController(IItemTypeService service)
        {
            _service = service;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int page)
        {
            var _itemtype = await _service.GetAll();

            int totalItem = _itemtype.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var Elements = _itemtype.Skip((page * 10)).Take(10).Select(x => new
            {
                ID = x.id,
                Name = x.name,
                Description = x.description,
                Created_at = x.created_at,
                Updated_at = x.updated_at
            }).ToList().OrderBy(_ => _.ID);

            var Response = new
            {
                Page = page,
                PageSize = 10,
                TotalItems = totalItem,
                TotalPages = totalPages,
                ItemType = Elements
            };
            return Ok(Response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var _itemtype = await _service.GetById(id);
            if (_itemtype == null) return NotFound($"ItemType with id {id} not found!");
            return Ok(_itemtype);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ItemTypeModel item)
        {
            if (item == null) return BadRequest("Itemtype is empty");
            await _service.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ItemTypeModel item)
        {
            if (item == null) return BadRequest(new { message = "Invalid warehouse data." });
            await _service.Update(id, item);
            return Ok(new { message = "ItemTypes has been updated!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return Ok(new { message = "Itemtype has been deleted!" });
        }
    }
}