using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controller
{
    [ApiController]
    [Route("api/v1/suppliers")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllSuppliers()
        {
            var suppliers = await _supplierService.GetAllSuppliers();
            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var supplier = await _supplierService.GetSupplierById(id);
            if (supplier == null) return NotFound($"Supplier with id {id} not found.");
            return Ok(supplier);
        }

        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetItemFromSupplierId(int id)
        {
            throw new NotImplementedException("Getting item from Supplier not implemented!");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SupplierModel supplier)
        {
            if (supplier == null) return BadRequest("Request is empty!");
            await _supplierService.UpdateSupplier(id, supplier);
            return Ok($"Supplier {supplier.name} has been updated!");
        }

        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] SupplierModel supplier)
        {
            if (supplier == null) return BadRequest("Request is empty!");
            await _supplierService.CreateSupplier(supplier);
            return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.id }, supplier);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            await _supplierService.DeleteSupplier(id);
            return Ok("Supplier has been deleted.");
        }
    }
}