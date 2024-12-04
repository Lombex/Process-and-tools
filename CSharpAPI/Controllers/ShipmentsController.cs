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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);
        return Ok("Shipment has been deleted!");
    }
}
}