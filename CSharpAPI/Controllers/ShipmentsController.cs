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

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        try
        {
            return Ok(_service.GetById(id));
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult Create([FromBody] ShipmentsModel shipment)
    {
        if (shipment == null)
            return BadRequest("Request is empty!");

        _service.Add(shipment);
        return CreatedAtAction(nameof(GetById), new { id = shipment.id }, shipment);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] ShipmentsModel shipment)
    {
        if (shipment == null)
            return BadRequest("Request is empty!");

        var result = _service.Update(id, shipment);
        if (!result)
            return NotFound($"Shipment {id} not found");

        return Ok("Shipment has been updated!");
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var result = _service.Delete(id);
        if (!result)
            return NotFound($"Shipment {id} not found");

        return Ok("Shipment has been deleted!");
    }
}
}