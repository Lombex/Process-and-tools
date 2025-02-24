using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/locations")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _service;
        private readonly IAuthService _authService;

        public LocationsController(ILocationService service, IAuthService authService)
        {
            _service = service;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "locations", method);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int page)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var locations = await _service.GetAll();

            int totalItem = locations.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var Elements = locations.Skip((page * 10)).Take(10).Select(x => new
            {
                ID = x.id,
                Warehouse_id = x.warehouse_id,
                Code = x.code,
                Name = x.name,
                Created_at = x.created_at,
                Updated_at = x.updated_at
            }).ToList().OrderBy(_ => _.ID);

            var Response = new
            {
                Page = page,
                PageSize = 10,
                TotalItems = totalItem,
                TotalPages = totalPages,
                Location = Elements
            };

            return Ok(Response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

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
            if (!await CheckAccess("GET"))
                return Forbid();

            var locations = await _service.GetByWarehouseId(warehouseId);
            return Ok(locations);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LocationModel location)
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            if (location == null) return BadRequest("Request is empty!");
            await _service.Add(location);
            return CreatedAtAction(nameof(GetById), new { id = location.id }, location);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LocationModel location)
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

            if (location == null) return BadRequest("Request is empty!");
            var result = await _service.Update(id, location);
            if (!result) return NotFound($"Location {id} not found");
            return Ok("Location has been updated!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await CheckAccess("DELETE"))
                return Forbid();

            var result = await _service.Delete(id);
            if (!result) return NotFound($"Location {id} not found");
            return Ok("Location has been deleted!");
        }
    }
}