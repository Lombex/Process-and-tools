using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface IOrderService
    {
        Task<List<OrderModel>> GetAllOrders();
        Task<OrderModel> GetOrderById(int id);
        Task<List<Items>> GetItemByOrderId(int id);
        Task UpdateOrders(int id, OrderModel updatedOrders);
        Task CreateOrder(OrderModel orders);
        Task DeleteOrder(int id);
        Task AddShipmentToOrder(int orderId, int shipmentId);
        Task RemoveShipmentFromOrder(int orderId, int shipmentId);
        Task<List<ShipmentModel>> GetShipmentsByOrderId(int orderId);
    }

    public class OrderService : IOrderService
    {
        private readonly SQLiteDatabase _Db;
        private readonly HistoryService _historyService;

        
        public OrderService(SQLiteDatabase sQLite, HistoryService historyService) 
        {
            _Db = sQLite;
            _historyService = historyService;
        }

        public async Task<List<OrderModel>> GetAllOrders() => 
            await _Db.Order.AsQueryable().ToListAsync();

        public async Task<OrderModel> GetOrderById(int id)
        {
            var _order = await _Db.Order.FirstOrDefaultAsync(x => x.id == id);
            if (_order == null) throw new Exception("Order not found!");
            return _order;
        }

        public async Task<List<Items>> GetItemByOrderId(int id)
        {
            var _order = await _Db.Order.FirstOrDefaultAsync(x => x.id == id);
            if (_order == null) throw new Exception("Order not found!");
            if (_order.items == null) throw new Exception("No items found!");
            return _order.items;
        }

        public async Task AddShipmentToOrder(int orderId, int shipmentId)
        {
            // Check if order exists
            var order = await GetOrderById(orderId);
            
            // Check if shipment exists
            var shipment = await _Db.Shipment.FindAsync(shipmentId);
            if (shipment == null)
                throw new Exception("Shipment not found!");

            // Check if mapping already exists
            var existingMapping = await _Db.OrderShipments
                .FirstOrDefaultAsync(m => m.OrderId == orderId && m.ShipmentId == shipmentId);
            
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

        public async Task RemoveShipmentFromOrder(int orderId, int shipmentId)
        {
            var mapping = await _Db.OrderShipments
                .FirstOrDefaultAsync(m => m.OrderId == orderId && m.ShipmentId == shipmentId);
                
            if (mapping != null)
            {
                _Db.OrderShipments.Remove(mapping);
                await _Db.SaveChangesAsync();
            }
        }

        public async Task<List<ShipmentModel>> GetShipmentsByOrderId(int orderId)
        {
            return await _Db.OrderShipments
                .Where(m => m.OrderId == orderId)
                .Join(_Db.Shipment,
                    m => m.ShipmentId,
                    s => s.id,
                    (m, s) => s)
                .ToListAsync();
        }

        public async Task UpdateOrders(int id, OrderModel updatedOrders)
        {
            var _order = await GetOrderById(id);

            string changes = $"";
            if (_order.reference != updatedOrders.reference) changes += $"Reference: {_order.reference} -> {updatedOrders.reference}; ";
            if (_order.order_status != updatedOrders.order_status) changes += $"Status: {_order.order_status} -> {updatedOrders.order_status}; ";
            if (_order.total_amount != updatedOrders.total_amount) changes += $"Total Amount: {_order.total_amount} -> {updatedOrders.total_amount}; ";

            _order.source_id = updatedOrders.source_id;
            _order.order_date = updatedOrders.order_date;
            _order.request_date = updatedOrders.request_date;
            _order.reference = updatedOrders.reference;
            _order.reference_extra = updatedOrders.reference_extra;
            _order.order_status = updatedOrders.order_status;
            _order.notes = updatedOrders.notes;
            _order.shipping_notes = updatedOrders.shipping_notes;
            _order.picking_notes = updatedOrders.picking_notes;
            _order.warehouse_id = updatedOrders.warehouse_id;
            _order.ship_to = updatedOrders.ship_to;
            _order.bill_to = updatedOrders.bill_to;
            _order.total_amount = updatedOrders.total_amount;
            _order.total_discount = updatedOrders.total_discount;
            _order.total_tax = updatedOrders.total_tax;
            _order.total_surcharge = updatedOrders.total_surcharge;
            _order.updated_at = DateTime.Now;
            _order.items = updatedOrders.items;

            _Db.Order.Update(_order);
            await _Db.SaveChangesAsync();
            await _historyService.LogAsync(EntityType.Order, id.ToString(), "Updated", $"Wijzigingen: {changes}");

        }

        public async Task CreateOrder(OrderModel orders)
        {
            if (orders == null) throw new ArgumentNullException(nameof(orders));

            if (!(await _Db.ClientModels.AnyAsync(client => client.id == orders.ship_to)))
                throw new Exception($"Shipping address with Client ID {orders.ship_to} does not exist.");

            if (!(await _Db.ClientModels.AnyAsync(client => client.id == orders.bill_to))) 
                throw new Exception($"Billing address with Client ID {orders.bill_to} does not exist.");

            orders.created_at = DateTime.UtcNow;
            orders.updated_at = DateTime.UtcNow;

            await _Db.Order.AddAsync(orders);
            await _historyService.LogAsync(EntityType.Order, orders.id.ToString(), "Created", $"Order {orders.reference} is aangemaakt met totaalbedrag {orders.total_amount}");
            await _Db.SaveChangesAsync();
        }

        public async Task DeleteOrder(int id)
        {
            var order = await GetOrderById(id);
            if (order == null) throw new Exception("Order not found!");

            // Maak een kopie in de archieftabel
            var archivedOrder = new ArchivedOrderModel
            {
                id = order.id,
                source_id = order.source_id,
                order_date = order.order_date,
                request_date = order.request_date,
                reference = order.reference,
                reference_extra = order.reference_extra,
                order_status = order.order_status,
                notes = order.notes,
                shipping_notes = order.shipping_notes,
                picking_notes = order.picking_notes,
                warehouse_id = order.warehouse_id,
                ship_to = order.ship_to,
                bill_to = order.bill_to,
                total_amount = order.total_amount,
                total_discount = order.total_discount,
                total_tax = order.total_tax,
                total_surcharge = order.total_surcharge,
                created_at = order.created_at,
                updated_at = order.updated_at,
                archived_at = DateTime.UtcNow,
                items = order.items
            };

            await _Db.ArchivedOrders.AddAsync(archivedOrder);

            // Log de verwijdering in de History-tabel
            await _historyService.LogAsync(
                EntityType.Order,
                id.ToString(),
                "Archived",
                $"Order {order.reference} is gearchiveerd in plaats van verwijderd."
            );

            // Verwijder de originele order
            _Db.Order.Remove(order);
            await _Db.SaveChangesAsync();
        }
    }
}
