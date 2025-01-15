using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controller
{
    [ApiController]
    [Route("api/v1/suppliers")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IAuthService _authService;

        public SupplierController(ISupplierService supplierService, IAuthService authService)
        {
            _supplierService = supplierService;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "suppliers", method);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllSuppliers()
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var suppliers = await _supplierService.GetAllSuppliers();
            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var supplier = await _supplierService.GetSupplierById(id);
            if (supplier == null) return NotFound($"Supplier with id {id} not found.");
            return Ok(supplier);
        }

        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetItemFromSupplierId(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var item = await _supplierService.GetItemFromSupplierId(id);
            if (item == null) return NotFound($"Item with id {id} not found.");
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SupplierModel supplier)
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

            if (supplier == null) return BadRequest("Request is empty!");
            await _supplierService.UpdateSupplier(id, supplier);
            return Ok($"Supplier {supplier.name} has been updated!");
        }

        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] SupplierModel supplier)
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            if (supplier == null) return BadRequest("Request is empty!");
            await _supplierService.CreateSupplier(supplier);
            return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.id }, supplier);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            if (!await CheckAccess("DELETE"))
                return Forbid();

            await _supplierService.DeleteSupplier(id);
            return Ok("Supplier has been deleted.");
        }
    }
}