using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult GetAll()
        {
            var itemLines = _service.GetAllItemLines();
            return Ok(itemLines);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var itemLine = _service.GetItemLineById(id);
                return Ok(itemLine);
            }
            catch (Exception)
            {
                return NotFound($"ItemLine with id {id} not found.");
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] ItemLineModel itemLine)
        {
            if (itemLine == null) 
                return BadRequest("Request is empty!");

            _service.CreateItemLine(itemLine);
            return CreatedAtAction(nameof(GetById), new { id = itemLine.id }, itemLine);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] ItemLineModel itemLine)
        {
            if (itemLine == null) 
                return BadRequest("Request is empty!");

            var updated = _service.UpdateItemLine(id, itemLine);
            if (!updated) 
                return NotFound($"ItemLine with id {id} not found.");

            return Ok($"ItemLine {id} has been updated!");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var deleted = _service.DeleteItemLine(id);
            if (!deleted) 
                return NotFound($"ItemLine with id {id} not found!");

            return Ok("ItemLine has been deleted.");
        }
    }
}