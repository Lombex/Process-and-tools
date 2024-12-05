using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controllers
{

[ApiController]
[Route("api/v1/shipments")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _service;

    public ShipmentsController(IShipmentService service)
    {
        _service = service;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var shipments = await _service.GetAll();
        return Ok(shipments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var shipment = await _service.GetById(id);
        if (shipment == null) return NotFound($"Shipment with id {id} not found.");
        return Ok(shipment);    
    }

    [HttpGet("{id}/items")]
    public async Task<IActionResult> GetItems(int id)
    {
        var items = await _service.GetItems(id);
        if (items == null) return NotFound($"Items with id {id} not found.");
        return Ok(items);
    }

    [HttpGet("{id}/orders")]
    public async Task<IActionResult> GetOrderByShipmentId(int id)
    {
        var order = await _service.GetOrderByshipmentId(id);
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShipmentModel shipment)
    {
        if (shipment == null) return BadRequest("Request is empty!");
        await _service.Add(shipment);
        return CreatedAtAction(nameof(GetById), new { id = shipment.id }, shipment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ShipmentModel shipment)
    {
        if (shipment == null) return BadRequest("Request is empty!");
        await _service.Update(id, shipment);
        return Ok("Shipment has been updated!");
    }

    [HttpPut("{id}/items")]
    public async Task<IActionResult> UpdateItems(int id, [FromBody] ShipmentModel shipment)
    {
        if (shipment == null) return BadRequest("Request is empty!");
        await _service.UpdateItems(id, shipment);
        return Ok("Updated");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);
        return Ok("Shipment has been deleted!");
    }
}
}