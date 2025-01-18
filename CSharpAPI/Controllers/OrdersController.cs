using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Xml;

namespace CSharpAPI.Controller
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrdersControllers : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrdersControllers(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrders();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrdersById(int id) 
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound($"Order with id {id} not found.");
            return Ok(order);
        }

        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetItemFromOrderId(int id)
        {
            var items = await _orderService.GetItemByOrderId(id);
            if (items == null) return NotFound("items not found!");
            return Ok(items);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrders(int id, [FromBody] OrderModel orders)
        {
            if (orders == null) return BadRequest("Request is empty!");
            await _orderService.UpdateOrders(id, orders);
            return Ok($"Transfer {id} has been updated!");
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderModel orders)
        {
            if (orders == null) return BadRequest("Request is empty!");
            await _orderService.CreateOrder(orders);
            return CreatedAtAction(nameof(GetOrdersById), new { id = orders.id }, orders);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            await _orderService.DeleteOrder(id);
            return Ok("Order has been deleted!");
        }
    }
}