using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface IShipmentService
    {
    Task<List<ShipmentModel>> GetAll();
    Task<ShipmentModel> GetById(int id);
    Task<List<Items>> GetItems(int id);
    Task UpdateItems(int id, ShipmentModel model);
    Task Add(ShipmentModel shipment);
    Task Update(int id, ShipmentModel shipment);
    Task Delete(int id);

    // Nieuwe methodes voor order management
    Task<List<OrderModel>> GetOrdersByShipmentId(int shipmentId);
    Task AddOrderToShipment(int shipmentId, int orderId);
    Task RemoveOrderFromShipment(int shipmentId, int orderId);
    }

    public class ShipmentService : IShipmentService
    {
        private readonly SQLiteDatabase _Db;
        private readonly HistoryService _historyService;

        public ShipmentService(SQLiteDatabase sQLite,HistoryService historyService)
        {
            _Db = sQLite;
            _historyService = historyService;
        }

        public async Task<List<ShipmentModel>> GetAll() => await _Db.Shipment.AsQueryable().ToListAsync();

        public async Task<ShipmentModel> GetById(int id)
        {
            var _shipment = await _Db.Shipment.FirstOrDefaultAsync(x => x.id == id);
            if (_shipment == null) throw new Exception("Shipment not found!");
            return _shipment;
        }

        public async Task<List<Items>> GetItems(int id)
        {
            var _shipment = await GetById(id);
            return _shipment.items;
        }

        public async Task UpdateItems(int id, ShipmentModel model)
        {
            var _shipment = await GetById(id);
            _shipment.items = model.items;

            _Db.Shipment.Update(_shipment);
            await _Db.SaveChangesAsync();
        } 

        public async Task Add(ShipmentModel shipment)
        {
            if (shipment == null) throw new ArgumentNullException(nameof(shipment));
            await _Db.Shipment.AddAsync(shipment);
            await _Db.SaveChangesAsync();
        }

        public async Task Update(int id, ShipmentModel shipment)
        {
            var _shipment = await GetById(id);

             string changes = $"";
            if (_shipment.shipment_status != shipment.shipment_status) changes += $"Status: {_shipment.shipment_status} -> {shipment.shipment_status}; ";
            if (_shipment.total_package_weight != shipment.total_package_weight) changes += $"Total Weight: {_shipment.total_package_weight} -> {shipment.total_package_weight}; ";

            _shipment.source_id = shipment.source_id;
            _shipment.order_date = shipment.order_date;
            _shipment.request_date = shipment.request_date;
            _shipment.shipment_date = shipment.shipment_date;
            _shipment.shipment_type = shipment.shipment_type;
            _shipment.shipment_status = shipment.shipment_status;
            _shipment.notes = shipment.notes;
            _shipment.carrier_code = shipment.carrier_code;
            _shipment.carrier_description = shipment.carrier_description;
            _shipment.service_code = shipment.service_code;
            _shipment.payment_type = shipment.payment_type;
            _shipment.transfer_mode = shipment.transfer_mode;
            _shipment.total_package_count = shipment.total_package_count;
            _shipment.total_package_weight = shipment.total_package_weight;
            _shipment.items = shipment.items;
            _shipment.updated_at = DateTime.Now;

            _Db.Shipment.Update(_shipment);
            await _Db.SaveChangesAsync();
            await _historyService.LogAsync(EntityType.Shipment, id.ToString(), "Updated", $"Wijzigingen: {changes}");

        }

        public async Task Delete(int id)
        {
            var _shipment = await GetById(id);
            _Db.Shipment.Remove(_shipment);
            await _Db.SaveChangesAsync();
            await _historyService.LogAsync(EntityType.Shipment, id.ToString(), "Deleted", $"Shipment {_shipment.id} met status {_shipment.shipment_status} is verwijderd");

        }

        public async Task<List<OrderModel>> GetOrdersByShipmentId(int shipmentId)
        {
            return await _Db.OrderShipments
                .Where(m => m.ShipmentId == shipmentId)
                .Join(_Db.Order,
                    m => m.OrderId,
                    o => o.id,
                    (m, o) => o)
                .ToListAsync();
        }
        public async Task AddOrderToShipment(int shipmentId, int orderId)
        {
            // Check if shipment exists
            var shipment = await GetById(shipmentId);
            
            // Check if order exists
            var order = await _Db.Order.FindAsync(orderId);
            if (order == null)
                throw new Exception("Order not found!");

            // Check if mapping already exists
            var existingMapping = await _Db.OrderShipments
                .FirstOrDefaultAsync(m => m.ShipmentId == shipmentId && m.OrderId == orderId);
            
            if (existingMapping == null)
            {
                var mapping = new OrderShipmentMapping
                {
                    OrderId = orderId,
                    ShipmentId = shipmentId,
                    CreatedAt = DateTime.UtcNow
                };

                await _Db.OrderShipments.AddAsync(mapping);
                await _Db.SaveChangesAsync();
            }
        }
        public async Task RemoveOrderFromShipment(int shipmentId, int orderId)
        {
            var mapping = await _Db.OrderShipments
                .FirstOrDefaultAsync(m => m.ShipmentId == shipmentId && m.OrderId == orderId);
                
            if (mapping != null)
            {
                _Db.OrderShipments.Remove(mapping);
                await _Db.SaveChangesAsync();
            }
        }

    }
}