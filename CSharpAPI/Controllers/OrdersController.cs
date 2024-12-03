using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Xml;

namespace CShartpAPI.Controller
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
        public IActionResult GetAllOrders()
        {
            var orders = _orderService.GetAllOrders();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public IActionResult GetOrdersById(int id) 
        {
            var order = _orderService.GetOrderById(id);
            if (order == null) return NotFound($"Order with id {id} not found.");
            return Ok(order);
        }

        [HttpGet("{id}/items")]
        public IActionResult GetItemFromOrderId(int id)
        {
            var items = _orderService.GetItemByOrderId(id);
            if (items == null) return NotFound("items not found!");
            return Ok(items);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateOrders(int id, [FromBody] OrderModel orders)
        {
            if (orders == null) return BadRequest("Request is empty!");
            var updatedOrders = _orderService.UpdateOrders(id, orders);
            if (!updatedOrders) return NotFound($"Transfer with id {id} not found.");
            return Ok($"Transfer {id} has been updated!");
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] OrderModel orders)
        {
            if (orders == null) return BadRequest("Request is empty!");
            _orderService.CreateOrder(orders);
            return CreatedAtAction(nameof(GetOrdersById), new { id = orders.id }, orders);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var order = _orderService.DeleteOrder(id);
            if (!order) return NotFound($"Order with id {id} not found!");
            return Ok("Order has been deleted!");
        }
    }
}