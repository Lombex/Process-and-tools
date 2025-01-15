using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/itemlines")]
    public class ItemLinesController : ControllerBase
    {
        private readonly IItemLineService _service;

        public ItemLinesController(IItemLineService service)
        {
            _service = service;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int page)
        {
            var itemLines = await _service.GetAllItemLines();

            int totalItem = itemLines.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var Elements = itemLines.Skip((page * 10)).Take(10).Select(x => new
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
                ItemLine = Elements
            };
            return Ok(Response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var itemLine = await _service.GetItemLineById(id);
                return Ok(itemLine);
            }
            catch (Exception)
            {
                return NotFound($"ItemLine with id {id} not found.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ItemLineModel itemLine)
        {
            if (itemLine == null)
                return BadRequest("Request is empty!");

            await _service.CreateItemLine(itemLine);
            return CreatedAtAction(nameof(GetById), new { id = itemLine.id }, itemLine);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ItemLineModel itemLine)
        {
            if (itemLine == null)
                return BadRequest("Request is empty!");

            var updated = await _service.UpdateItemLine(id, itemLine);
            if (!updated)
                return NotFound($"ItemLine with id {id} not found.");

            return Ok($"ItemLine {id} has been updated!");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteItemLine(id);
            if (!deleted)
                return NotFound($"ItemLine with id {id} not found!");

            return Ok("ItemLine has been deleted.");
        }
    }
}