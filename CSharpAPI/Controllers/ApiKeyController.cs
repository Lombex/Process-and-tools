using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/keys")]
    public class ApiKeysController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly IAuthService _authService;

        public ApiKeysController(IApiKeyService apiKeyService, IAuthService authService)
        {
            _apiKeyService = apiKeyService;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "keys", method);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ApiUser>>> GetAllKeys()
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var user = HttpContext.Items["User"] as ApiUser;
            var keys = await _apiKeyService.GetAllApiKeys(user);
            return Ok(keys);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiUser>> GetKeyById(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var user = HttpContext.Items["User"] as ApiUser;
            var key = await _apiKeyService.GetApiKeyById(id, user);
            if (key == null)
            {
                return NotFound($"API key with id {id} not found.");
            }
            return Ok(key);
        }

        [HttpPost]
        public async Task<ActionResult<ApiUser>> CreateKey([FromBody] ApiUser apiUser)
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            if (apiUser == null)
                return BadRequest("API key data is null.");

            var currentUser = HttpContext.Items["User"] as ApiUser;
            
            // Only Admin can create Admin keys
            if (apiUser.role == "Admin" && currentUser.role != "Admin")
                return Forbid("Only Admin users can create Admin keys");

            // Users can only create keys for their warehouse unless they're Admin
            if (currentUser.role != "Admin" && apiUser.warehouse_id != currentUser.warehouse_id)
                return Forbid("Can only create keys for your assigned warehouse");

            await _apiKeyService.AddApiKey(apiUser);
            return CreatedAtAction(nameof(GetKeyById), new { id = apiUser.id }, apiUser);
        }

        [HttpPost("generate")]
        public async Task<ActionResult<ApiUser>> GenerateKey([FromBody] KeyGenerationRequest request)
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            if (request == null)
                return BadRequest("Request data is null.");

            var currentUser = HttpContext.Items["User"] as ApiUser;
            
            // Only Admin can create Admin keys
            if (request.role == "Admin" && currentUser.role != "Admin")
                return Forbid("Only Admin users can create Admin keys");

            // Users can only create keys for their warehouse unless they're Admin
            if (currentUser.role != "Admin" && request.warehouse_id != currentUser.warehouse_id)
                return Forbid("Can only create keys for your assigned warehouse");

            // Generate a new API key
            var apiKey = await _apiKeyService.GenerateApiKey(request);
            return Ok(apiKey);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKey(int id, [FromBody] ApiUser apiUser)
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

            if (apiUser == null)
                return BadRequest("Invalid API key data.");

            var currentUser = HttpContext.Items["User"] as ApiUser;
            
            // Can't update Admin keys unless you're Admin
            var existingKey = await _apiKeyService.GetApiKeyById(id, currentUser);
            if (existingKey == null)
                return NotFound($"API key with id {id} not found.");

            if (existingKey.role == "Admin" && currentUser.role != "Admin")
                return Forbid("Only Admin users can modify Admin keys");

            // Users can only update keys for their warehouse unless they're Admin
            if (currentUser.role != "Admin" && existingKey.warehouse_id != currentUser.warehouse_id)
                return Forbid("Can only modify keys for your assigned warehouse");

            await _apiKeyService.UpdateApiKey(id, apiUser);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKey(int id)
        {
            if (!await CheckAccess("DELETE"))
                return Forbid();

            var currentUser = HttpContext.Items["User"] as ApiUser;
            var keyToDelete = await _apiKeyService.GetApiKeyById(id, currentUser);
            
            if (keyToDelete == null)
                return NotFound($"API key with id {id} not found.");

            // Can't delete Admin keys unless you're Admin
            if (keyToDelete.role == "Admin" && currentUser.role != "Admin")
                return Forbid("Only Admin users can delete Admin keys");

            // Users can only delete keys for their warehouse unless they're Admin
            if (currentUser.role != "Admin" && keyToDelete.warehouse_id != currentUser.warehouse_id)
                return Forbid("Can only delete keys for your assigned warehouse");

            await _apiKeyService.DeleteApiKey(id);
            return NoContent();
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetAvailableRoles()
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var roles = new[]
            {
                "Admin", "Warehouse_Manager", "Inventory_Manager", 
                "Floor_Manager", "Operative", "Supervisor",
                "Analyst", "Logistics", "Sales"
            };

            return Ok(roles);
        }
    }

    public class KeyGenerationRequest
    {
        public string role { get; set; }
        public string app { get; set; }
        public int? warehouse_id { get; set; }
    }
}