using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/itemgroup")]
    public class ItemGroupsController : ControllerBase
    {
        private readonly IItemGroupService _service;
        private readonly IAuthService _authService;

        public ItemGroupsController(IItemGroupService service, IAuthService authService)
        {
            _service = service;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "itemgroup", method);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var itemGroups = await _service.GetAll();
            return Ok(itemGroups);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

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
            if (!await CheckAccess("POST"))
                return Forbid();

            if (itemGroup == null)
                return BadRequest("Request body is empty!");

            await _service.Add(itemGroup);
            return CreatedAtAction(nameof(GetById), new { id = itemGroup.id }, itemGroup);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ItemGroupModel itemGroup)
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

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
            if (!await CheckAccess("DELETE"))
                return Forbid();

            var deleted = await _service.Delete(id);
            if (!deleted)
                return NotFound($"ItemGroup {id} not found");

            return Ok("ItemGroup has been deleted.");
        }
    }
}