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
        public async Task<IActionResult> GetAll()
        {
            var _itemtype = await _service.GetAll();
            return Ok(_itemtype);
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