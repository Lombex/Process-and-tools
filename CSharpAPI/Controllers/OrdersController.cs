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
        public async Task<IActionResult> GetAllOrders()
        {
            if (!await CheckAccess("GET"))
                return Forbid();

            var orders = await _orderService.GetAllOrders();
            return Ok(orders);
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