using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controller 
{
    [ApiController]
    [Route("api/v1/transfers")]
    public class TransferController : ControllerBase 
    {
        private readonly ITransfersService _transferSerivces;
        private readonly IAuthService _authService;

        public TransferController(ITransfersService transfersService, IAuthService authService)
        {
            _transferSerivces = transfersService;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "transfers", method);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllTransfers([FromQuery] int page)
        {
            var transfers = await _transferSerivces.GetAllTransfers();

            int totalItem = transfers.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var Elements = transfers.Skip((page * 10)).Take(10).Select(x => new
            {
                ID = x.id,
                Reference = x.reference,
                Transfer_from = x.transfer_from,
                Transfer_to = x.transfer_to,
                Transer_Status = x.transfer_status,
                Created_at = x.created_at,
                Updated_at = x.updated_at,
                Items = x.items
            }).ToList().OrderBy(_ => _.ID);

            var Response = new
            {
                Page = page,
                PageSize = 10,
                TotalItems = totalItem,
                TotalPages = totalPages,
                Transfers = Elements
            };

            return Ok(Response);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransferById(int id) 
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var transfer = await _transferSerivces.GetTransferById(id);
            if (transfer == null) return NotFound($"Transfer with id {id} not found.");
            return Ok(transfer);
        }

        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetItemFromTransferId(int id) 
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var items = await _transferSerivces.GetItemFromTransferId(id);
            if (items == null) return NotFound($"Item with id {id} not found.");
            return Ok(items);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransfer(int id, [FromBody] TransferModel transfer) 
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

            if (transfer == null) return BadRequest("Request is empty!");
            await _transferSerivces.UpdateTransfer(id, transfer);
            return Ok($"Transfer {id} has been updated!");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransfer([FromBody] TransferModel transfer) 
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            if (transfer == null) return BadRequest("Request is empty!");
            await _transferSerivces.CreateTransfer(transfer);
            return CreatedAtAction(nameof(GetTransferById), new { id = transfer.id}, transfer);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeteteTransfer(int id) 
        {
            if (!await CheckAccess("DELETE"))
                return Forbid();

            await _transferSerivces.DeleteTransfer(id);
            return Ok("Transfer has been deleted.");
        }
        
        [HttpPost("{id}/commit")]
        public async Task<IActionResult> CommitTransfer(int id)
        {
            try
            {
                await _transferSerivces.CommitTransfer(id);
                return Ok($"Transfer {id} has been Completed and processed.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing transfer {id}: {ex.Message}");
            }
        }

        [HttpGet("{id}/to/{location}")]
        public async Task<IActionResult> TransferToLocation(int id, int location)
        {
            try
            {
                await _transferSerivces.TransferToLocation(id, location);
                return Ok($"Transfer {id} has been processed to location {location}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing transfer {id}: {ex.Message}");
            }
        }

        [HttpGet("{id}/from/{location}")]

        public async Task<IActionResult> TransferFromLocation(int id, int location)
        {
            try
            {
                await _transferSerivces.TransferFromLocation(id, location);
                return Ok($"Transfer {id} has been processed from location {location}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing transfer {id}: {ex.Message}");
            }
        }



    }
}
