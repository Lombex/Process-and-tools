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

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_service.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                return Ok(_service.GetById(id));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] ItemTypeModel item)
        {
            if (item == null) return BadRequest();
            _service.Add(item);
            return Ok(item);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] ItemTypeModel item)
        {
            if (item == null) return BadRequest();
            var result = _service.Update(id, item);
            if (!result) return NotFound($"ItemType {id} not found");
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var result = _service.Delete(id);
            if (!result) return NotFound($"ItemType {id} not found");
            return Ok();
        }
    }
}