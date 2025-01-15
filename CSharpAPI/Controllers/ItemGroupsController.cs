using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/itemgroup")]
    public class ItemGroupsController : ControllerBase
    {
        private readonly IItemGroupService _service;

        public ItemGroupsController(IItemGroupService service)
        {
            _service = service;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int page)
        {
            var itemGroups = await _service.GetAll();
            int totalItem = itemGroups.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var Elements = itemGroups.Skip((page * 10)).Take(10).Select(x => new
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
                ItemGroup = Elements
            };
            return Ok(Response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var itemGroup = await _service.GetById(id);
                return Ok(itemGroup);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ItemGroupModel itemGroup)
        {
            if (itemGroup == null)
                return BadRequest("Request body is empty!");

            await _service.Add(itemGroup);
            return CreatedAtAction(nameof(GetById), new { id = itemGroup.id }, itemGroup);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ItemGroupModel itemGroup)
        {
            if (itemGroup == null)
                return BadRequest("Request body is empty!");

            var updated = await _service.Update(id, itemGroup);
            if (!updated)
                return NotFound($"ItemGroup {id} not found");

            return Ok($"ItemGroup {id} has been updated!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.Delete(id);
            if (!deleted)
                return NotFound($"ItemGroup {id} not found");

            return Ok("ItemGroup has been deleted.");
        }
    }
}