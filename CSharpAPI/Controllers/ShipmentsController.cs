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
    public async Task<IActionResult> GetAll([FromQuery] int page)
    {
        var shipments = await _service.GetAll();

        int totalItem = shipments.Count;
        int totalPages = (int)Math.Ceiling(totalItem / (double)10);
        if (page > totalPages) return BadRequest("Page number exceeds total pages");

        var Elements = shipments.Skip((page * 10)).Take(10).Select(x => new
        {
            ID = x.id,
            Order_id = x.order_id,
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