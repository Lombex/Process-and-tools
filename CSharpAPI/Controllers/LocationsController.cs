using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/locations")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _service;

        public LocationsController(ILocationService service)
        {
            _service = service;
        }

        [HttpGet("all")]
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

        [HttpGet("warehouse/{warehouseId}")]
        public IActionResult GetByWarehouseId(int warehouseId)
        {
            return Ok(_service.GetByWarehouseId(warehouseId));
        }

        [HttpPost]
        public IActionResult Create([FromBody] LocationModel location)
        {
            if (location == null) return BadRequest("Request is empty!");
            _service.Add(location);
            return CreatedAtAction(nameof(GetById), new { id = location.id }, location);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] LocationModel location)
        {
            if (location == null) return BadRequest("Request is empty!");
            var result = _service.Update(id, location);
            if (!result) return NotFound($"Location {id} not found");
            return Ok("Location has been updated!");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var result = _service.Delete(id);
            if (!result) return NotFound($"Location {id} not found");
            return Ok("Location has been deleted!");
        }
    }
}