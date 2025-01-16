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
        public async Task<IActionResult> GetAllSuppliers([FromQuery] int page)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var suppliers = await _supplierService.GetAllSuppliers();

            int totalItem = suppliers.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var Elements = suppliers.Skip((page * 10)).Take(10).Select(x => new
            {
                ID = x.id,
                Code = x.code,
                Name = x.name,
                Address = x.address,
                Address_Extra = x.address_extra,
                City = x.city,
                Zip_Code = x.zip_code,
                Province = x.province,
                Contact = x.contact,
                Reference = x.reference,
                Created_at = x.created_at,
                Updated_at = x.updated_at
            }).ToList().OrderBy(_ => _.ID);

            var Response = new
            {
                Page = page,
                PageSize = 10,
                TotalItems = totalItem,
                TotalPages = totalPages,
                Suppliers = Elements
            };

            return Ok(Response);
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