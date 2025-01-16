using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CShartpAPI.Controller
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrdersControllers : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IAuthService _authService;

        public OrdersControllers(IOrderService orderService, IAuthService authService)
        {
            _orderService = orderService;
            _authService = authService;
        }

        private async Task<bool> CheckAccess(string method)
        {
            var user = HttpContext.Items["User"] as ApiUser;
            return await _authService.HasAccess(user, "orders", method);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int page)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var orders = await _orderService.GetAllOrders();
            int totalItem = orders.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages");

            var Elements = orders.Skip((page * 10)).Take(10).Select(x => new
            {
                ID = x.id,
                Source_id = x.source_id,
                Order_date = x.order_date,
                Request_date = x.request_date,
                Reference = x.reference,
                Reference_extra = x.reference_extra,
                Order_status = x.order_status,
                Notes = x.notes,
                Shipping_notes = x.shipping_notes,
                Picking_notes = x.picking_notes,
                Warehouse_id = x.warehouse_id,
                Ship_to = x.ship_to,
                Bill_to = x.bill_to,
                Shipment_id = x.shipment_id,
                Total_amount = x.total_amount,
                Total_discount = x.total_discount,
                Total_tax = x.total_tax,
                Total_Surcharge = x.total_surcharge,
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
                Order = Elements
            };

            return Ok(Response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrdersById(int id) 
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound($"Order with id {id} not found.");
            return Ok(order);
        }

        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetItemFromOrderId(int id)
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var items = await _orderService.GetItemByOrderId(id);
            if (items == null) return NotFound("items not found!");
            return Ok(items);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrders(int id, [FromBody] OrderModel orders)
        {
            if (!await CheckAccess("PUT"))
                return Forbid();

            if (orders == null) return BadRequest("Request is empty!");
            await _orderService.UpdateOrders(id, orders);
            return Ok($"Transfer {id} has been updated!");
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderModel orders)
        {
            if (!await CheckAccess("POST"))
                return Forbid();

            if (orders == null) return BadRequest("Request is empty!");
            await _orderService.CreateOrder(orders);
            return CreatedAtAction(nameof(GetOrdersById), new { id = orders.id }, orders);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            if (!await CheckAccess("DELETE"))
                return Forbid();

            await _orderService.DeleteOrder(id);
            return Ok("Order has been deleted!");
        }
    }
}