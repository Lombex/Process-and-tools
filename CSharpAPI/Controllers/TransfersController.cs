using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controller {
    
    [ApiController]
    [Route("api/v1/transfers")]
    public class TransferController : ControllerBase {
        private readonly ITransfersService _transferSerivces;
        public TransferController(ITransfersService transfersService){
            _transferSerivces = transfersService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllTransfers() {
            var transfers = await _transferSerivces.GetAllTransfers();
            return Ok(transfers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransferById(int id) {
            var transfer = await _transferSerivces.GetTransferById(id);
            if (transfer == null) return NotFound($"Transfer with id {id} not found.");
            return Ok(transfer);
        }

        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetItemFromTransferId(int id) {
            var items = await _transferSerivces.GetItemFromTransferId(id);
            if (items == null) return NotFound($"Item with id {id} not found.");
            return Ok(items);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransfer(int id, [FromBody] TransferModel transfer) {
            if (transfer == null) return BadRequest("Request is empty!");
            await _transferSerivces.UpdateTransfer(id, transfer);
            return Ok($"Transfer {id} has been updated!");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransfer([FromBody] TransferModel transfer) {
            if (transfer == null) return BadRequest("Request is empty!");
            await _transferSerivces.CreateTransfer(transfer);
            return CreatedAtAction(nameof(GetTransferById), new { id = transfer.id}, transfer);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeteteTransfer(int id) {
            await _transferSerivces.DeleteTransfer(id);
            return Ok("Transfer has been deleted.");
        }
    }
}