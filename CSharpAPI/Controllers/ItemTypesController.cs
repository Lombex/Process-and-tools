using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/itemtypes")]
    public class ItemTypesController : ControllerBase
    {
        private readonly IItemTypeService _service;
        private readonly IAuthService _authService;

        public ItemTypesController(IItemTypeService service, IAuthService authService)
        {
            _service = service;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "itemtypes", method);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var _itemtype = await _service.GetAll();
            return Ok(_itemtype);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var _itemtype = await _service.GetById(id);
            if (_itemtype == null) return NotFound($"ItemType with id {id} not found!");
            return Ok(_itemtype);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ItemTypeModel item)
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            if (item == null) return BadRequest("Itemtype is empty");
            await _service.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ItemTypeModel item)
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

            if (item == null) return BadRequest(new { message = "Invalid warehouse data." });
            await _service.Update(id, item);
            return Ok(new { message = "ItemTypes has been updated!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await CheckAccess("DELETE"))
                return Forbid();

            await _service.Delete(id);
            return Ok(new { message = "Itemtype has been deleted!" });
        }
    }
}