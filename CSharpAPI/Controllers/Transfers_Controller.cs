using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controller {
    
    [Route("api/v1/transfers")]
    public class TransferController : ControllerBase {
        private readonly ITransfersService _transferSerivces;
        public TransferController(ITransfersService transfersService){
            _transferSerivces = transfersService;
        }

        [HttpGet()]
        public IActionResult GetAllTransfers() {
            var transfers = _transferSerivces.GetAllTransfers();
            return Ok(transfers);
        }

        [HttpGet("{id}")]
        public IActionResult GetTransferById(int id) {
            var transfer = _transferSerivces.GetTransferById(id);
            if (transfer == null) return NotFound($"Transfer with id {id} not found.");
            return Ok(transfer);
        }

        [HttpGet("{id}/items")]
        public IActionResult GetItemFromTransferId(int id) {
            var items = _transferSerivces.GetItemFromTransferId(id);
            if (items == null) return NotFound($"Item with id {id} not found.");
            return Ok(items);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTransfer(int id, [FromBody] TransferModel transfer) {
            if (transfer == null) return BadRequest("Request is empty!");
            var updatedTransfer = _transferSerivces.UpdateTransfer(id, transfer);
            if (!updatedTransfer) return NotFound($"Transfer with id {id} not found.");
            return Ok($"Transfer {id} has been updated!");
        }

        [HttpPost]
        public IActionResult CreateTransfer([FromBody] TransferModel transfer) {
            if (transfer == null) return BadRequest("Request is empty!");
            _transferSerivces.CreateTransfer(transfer);
            return CreatedAtAction(nameof(GetTransferById), new { id = transfer.id}, transfer);
        }

        [HttpDelete("{id}")]
        public IActionResult DeteteTransfer(int id) {
            var transfer = _transferSerivces.DeleteTransfer(id);
            if (!transfer) return NotFound($"Transfer with id {id} not found!");
            return Ok("Transfer has been deleted.");
        }
    }
}