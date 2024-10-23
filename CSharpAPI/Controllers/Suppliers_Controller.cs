using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controller
{
    [Route("api/v1/suppliers")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet()]
        public IActionResult GetAllSuppliers()
        {
            var suppliers = _supplierService.GetAllSuppliers();
            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        public IActionResult GetSupplierById(int id)
        {
            var supplier = _supplierService.GetSupplierById(id);
            if (supplier == null) return NotFound($"Supplier with id {id} not found.");
            return Ok(supplier);
        }

        [HttpGet("{id}/items")]
        public IActionResult GetItemFromSupplierId(int id)
        {
            throw new NotImplementedException("Getting item from Supplier not implemented!");
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSupplier(int id, [FromBody] SuppliersModel supplier)
        {
            if (supplier == null) return BadRequest("Request is empty!");
            var updateSupplier = _supplierService.UpdateSupplier(id, supplier);
            if (!updateSupplier) return NotFound($"Supplier with id {id} not found!");
            return Ok($"Supplier {supplier.name} has been updated!");
        }

        [HttpPost]
        public IActionResult CreateSupplier([FromBody] SuppliersModel supplier)
        {
            if (supplier == null) return BadRequest("Request is empty!");
            _supplierService.CreateSupplier(supplier);
            return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.id }, supplier);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSupplier(int id)
        {
            var supplier = _supplierService.DeleteSupplier(id);
            if (!supplier) return NotFound($"Supplier with id {id} not found!");
            return Ok("Supplier has been deleted.");
        }
    }
}