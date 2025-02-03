using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Services.V2;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;

namespace CSharpAPI.Controllers.V2
{
    [ApiController]
    [Route("api/v2/inventory-locations")]
    public class InventoryLocationsController : ControllerBase
    {
        private readonly IInventoryLocationService _service;
        private readonly IAuthService _authService;

        public InventoryLocationsController(IInventoryLocationService service, IAuthService authService)
        {
            _service = service;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "inventories", method);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 0)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var locations = await _service.GetAll();

            int totalItems = locations.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var elements = locations.Skip(page * 10).Take(10).Select(x => new
            {
                Id = x.Id,
                InventoryId = x.InventoryId,
                LocationId = x.LocationId,
                Amount = x.Amount,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList();

            var response = new
            {
                Page = page,
                PageSize = 10,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Locations = elements
            };

            return Ok(response);
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InventoryLocationModel model)
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            if (model == null)
                return BadRequest("Request body is empty");

            try
            {
                var result = await _service.Create(model);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InventoryLocationModel model)
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

            if (model == null)
                return BadRequest("Request body is empty");

            try
            {
                var result = await _service.Update(id, model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await CheckAccess("DELETE"))
                return Forbid();

            try
            {
                await _service.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}