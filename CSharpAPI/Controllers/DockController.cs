using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;

namespace CSharpAPI.Controller
{
    [Route("api/v1/docks")]
    [ApiController]
    public class DockController : ControllerBase
    {
        private readonly IDockService _dockService;
        private readonly IAuthService _authService;

        public DockController(IDockService dockService, IAuthService authService)
        {
            _dockService = dockService;
            _authService = authService;
        }

        private async Task<IActionResult> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            var hasAccess = await _authService.HasAccess(user, "docks", method);
            if (!hasAccess)
            {
                return StatusCode(403, new { message = "Access denied" });
            }
            return null;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int page)
        {
            var accessCheck = await CheckAccess("GET");
            if (accessCheck != null)
                return accessCheck;

            var docks = await _dockService.GetAllDocks();

            int totalItem = docks.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var elements = docks.Skip((page * 10)).Take(10).Select(x => new
            {
                ID = x.id,
                Code = x.code,
                Name = x.name,
                CreatedAt = x.created_at,
                UpdatedAt = x.updated_at
            }).ToList().OrderBy(_ => _.ID);

            var response = new
            {
                Page = page,
                PageSize = 10,
                TotalItems = totalItem,
                TotalPages = totalPages,
                Docks = elements
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var accessCheck = await CheckAccess("GET");
            if (accessCheck != null)
                return accessCheck;

            var dock = await _dockService.GetDockById(id);
            if (dock == null) return NotFound($"Dock with id {id} not found.");
            return Ok(dock);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DockModel dock)
        {
            var accessCheck = await CheckAccess("POST");
            if (accessCheck != null)
                return accessCheck;

            if (dock == null) return BadRequest("Dock is null.");
            await _dockService.AddDock(dock);
            return CreatedAtAction(nameof(Get), new { id = dock.id }, dock);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] DockModel dock)
        {
            var accessCheck = await CheckAccess("PUT");
            if (accessCheck != null)
                return accessCheck;

            if (dock == null) return BadRequest(new { message = "Invalid dock data." });
            await _dockService.UpdateDock(id, dock);
            return Ok("Dock has been updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var accessCheck = await CheckAccess("DELETE");
            if (accessCheck != null)
                return accessCheck;

            await _dockService.DeleteDock(id);
            return Ok("Dock has been deleted");
        }


        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouseId(int warehouseId)
        {
            var accessCheck = await CheckAccess("GET");
            if (accessCheck != null)
                return accessCheck;

            var dock = await _dockService.GetDockByWarhouseId(warehouseId);
            if (dock == null) return NotFound($"Dock with warehouse id {warehouseId} not found.");
            return Ok(dock);
        }
    }
}
