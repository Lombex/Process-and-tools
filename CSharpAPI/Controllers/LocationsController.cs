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
        public async Task<IActionResult> GetAll()
        {
            var locations = await _service.GetAll();
            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var location = await _service.GetById(id);
                return Ok(location);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouseId(int warehouseId)
        {
            var locations = await _service.GetByWarehouseId(warehouseId);
            return Ok(locations);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LocationModel location)
        {
            if (location == null) return BadRequest("Request is empty!");
            await _service.Add(location);
            return CreatedAtAction(nameof(GetById), new { id = location.id }, location);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LocationModel location)
        {
            if (location == null) return BadRequest("Request is empty!");
            var result = await _service.Update(id, location);
            if (!result) return NotFound($"Location {id} not found");
            return Ok("Location has been updated!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            if (!result) return NotFound($"Location {id} not found");
            return Ok("Location has been deleted!");
        }
    }
}