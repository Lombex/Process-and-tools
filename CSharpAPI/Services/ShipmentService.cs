using CSharpAPI.Models;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
   public interface IShipmentService
{
    List<ShipmentsModel> GetAll();
    ShipmentsModel GetById(int id);
    void Add(ShipmentsModel shipment);
    bool Update(int id, ShipmentsModel shipment);
    bool Delete(int id);
}

public class ShipmentService : IShipmentService
{
    private readonly string dataPath = "data/shipments.json";

    public List<ShipmentsModel> GetAll()
    {
        if (!File.Exists(dataPath))
            return new List<ShipmentsModel>();

        var jsonContent = File.ReadAllText(dataPath);
        return JsonConvert.DeserializeObject<List<ShipmentsModel>>(jsonContent) ?? new List<ShipmentsModel>();
    }

    public ShipmentsModel GetById(int id)
    {
        var shipment = GetAll().FirstOrDefault(x => x.id == id);
        if (shipment == null)
            throw new Exception($"Shipment {id} not found");
        return shipment;
    }

    public void Add(ShipmentsModel shipment)
    {
        var items = GetAll();
        shipment.id = items.Count > 0 ? items.Max(x => x.id) + 1 : 1;
        shipment.created_at = DateTime.UtcNow;
        shipment.updated_at = DateTime.UtcNow;
        items.Add(shipment);
        SaveToFile(items);
    }

    public bool Update(int id, ShipmentsModel shipment)
    {
        var items = GetAll();
        var existing = items.FirstOrDefault(x => x.id == id);
        if (existing == null)
            return false;

        existing.order_id = shipment.order_id;
        existing.source_id = shipment.source_id;
        existing.order_date = shipment.order_date;
        existing.request_date = shipment.request_date;
        existing.shipment_date = shipment.shipment_date;
        existing.shipment_type = shipment.shipment_type;
        existing.shipment_status = shipment.shipment_status;
        existing.notes = shipment.notes;
        existing.carrier_code = shipment.carrier_code;
        existing.carrier_description = shipment.carrier_description;
        existing.service_code = shipment.service_code;
        existing.payment_type = shipment.payment_type;
        existing.transfer_mode = shipment.transfer_mode;
        existing.total_package_count = shipment.total_package_count;
        existing.total_package_weight = shipment.total_package_weight;
        existing.items = shipment.items;
        existing.updated_at = DateTime.UtcNow;

        SaveToFile(items);
        return true;
    }

    public bool Delete(int id)
    {
        var items = GetAll();
        var item = items.FirstOrDefault(x => x.id == id);
        if (item == null)
            return false;

        items.Remove(item);
        SaveToFile(items);
        return true;
    }

    private void SaveToFile(List<ShipmentsModel> items)
    {
        var jsonContent = JsonConvert.SerializeObject(items, Formatting.Indented);
        File.WriteAllText(dataPath, jsonContent);
    }
}
}