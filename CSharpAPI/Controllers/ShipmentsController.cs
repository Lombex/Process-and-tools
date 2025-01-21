using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v1/shipments")]
    public class ShipmentsController : ControllerBase
    {
        private readonly IShipmentService _service;
        private readonly IAuthService _authService;

        public ShipmentsController(IShipmentService service, IAuthService authService)
        {
            _service = service;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "shipments", method);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int page)
        {
            var shipments = await _service.GetAll();

            int totalItem = shipments.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var Elements = shipments.Skip((page * 10)).Take(10).Select(x => new
            {
                ID = x.id,
                Source_id = x.source_id,
                Order_date = x.order_date,
                Request_date = x.request_date,
                Shipment_date = x.shipment_date,
                Shipment_type = x.shipment_type,
                Shipment_Status = x.shipment_status,
                Notes = x.notes,
                Carrier_code = x.carrier_code,
                Carrier_description = x.carrier_description,
                Service_code = x.service_code,
                Payment_type = x.payment_type,
                Transfer_mode = x.transfer_mode,
                Total_package_count = x.total_package_count,
                Total_package_weight = x.total_package_weight,
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
                Shipments = Elements
            };

            return Ok(Response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var shipment = await _service.GetById(id);
            if (shipment == null) return NotFound($"Shipment with id {id} not found.");
            return Ok(shipment);    
        }

        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetItems(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var items = await _service.GetItems(id);
            if (items == null) return NotFound($"Items with id {id} not found.");
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ShipmentModel shipment)
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            if (shipment == null) return BadRequest("Request is empty!");
            await _service.Add(shipment);
            return CreatedAtAction(nameof(GetById), new { id = shipment.id }, shipment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ShipmentModel shipment)
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

            if (shipment == null) return BadRequest("Request is empty!");
            await _service.Update(id, shipment);
            return Ok("Shipment has been updated!");
        }

        [HttpPut("{id}/items")]
        public async Task<IActionResult> UpdateItems(int id, [FromBody] ShipmentModel shipment)
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

            if (shipment == null) return BadRequest("Request is empty!");
            await _service.UpdateItems(id, shipment);
            return Ok("Updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await CheckAccess("DELETE"))
                return Forbid();

            await _service.Delete(id);
            return Ok("Shipment has been deleted!");
        }

        [HttpGet("{id}/orders")]
        public async Task<IActionResult> GetShipmentOrders(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            try
            {
                var orders = await _service.GetOrdersByShipmentId(id);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{shipmentId}/orders/{orderId}")]
        public async Task<IActionResult> AddOrderToShipment(int shipmentId, int orderId)
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            try
            {
                await _service.AddOrderToShipment(shipmentId, orderId);
                return Ok("Order added to shipment successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{shipmentId}/orders/{orderId}")]
        public async Task<IActionResult> RemoveOrderFromShipment(int shipmentId, int orderId)
        {
            if (!await CheckAccess("DELETE"))
                return Forbid();

            try
            {
                await _service.RemoveOrderFromShipment(shipmentId, orderId);
                return Ok("Order removed from shipment successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}