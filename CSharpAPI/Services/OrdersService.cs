using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Services;
using CSharpAPI.Services.V2;
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
        private readonly IInventoryLocationService  _inventoryLocationService;

        public OrderService(SQLiteDatabase sQLite, HistoryService historyService, IInventoryLocationService  inventoryLocationService)
        {
            _Db = sQLite;
            _historyService = historyService;
            _inventoryLocationService = inventoryLocationService;
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

        public async Task UpdateOrders(int id, OrderModel updatedOrder)
        {
            var existingOrder = await GetOrderById(id);

            foreach (var updatedItem in updatedOrder.items)
            {
                var originalItem = existingOrder.items.FirstOrDefault(i => i.item_id == updatedItem.item_id);

                if (originalItem != null && originalItem.amount != updatedItem.amount)
                {
                    int quantityChange = updatedItem.amount - originalItem.amount;

                    if (quantityChange > 0)
                    {
                        await _inventoryLocationService.PlaceOrder(updatedItem.item_id, quantityChange);
                    }
                    else
                    {
                        await _inventoryLocationService.RemoveOrder(updatedItem.item_id, -quantityChange);
                    }
                }
            }

            _Db.Order.Update(existingOrder);
            await _Db.SaveChangesAsync();
            await _historyService.LogAsync(EntityType.Order, id.ToString(), "Updated", $"Order {existingOrder.reference} updated.");
        }

        public async Task CreateOrder(OrderModel order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            // Validate that order references exist
            if (!(await _Db.ClientModels.AnyAsync(client => client.id == order.ship_to)))
                throw new Exception($"Shipping address with Client ID {order.ship_to} does not exist.");

            if (!(await _Db.ClientModels.AnyAsync(client => client.id == order.bill_to)))
                throw new Exception($"Billing address with Client ID {order.bill_to} does not exist.");

            order.created_at = DateTime.UtcNow;
            order.updated_at = DateTime.UtcNow;

            await _Db.Order.AddAsync(order);
            await _Db.SaveChangesAsync();

            // Allocate inventory
            foreach (var item in order.items)
            {
                await _inventoryLocationService.PlaceOrder(item.item_id, item.amount);
            }

            await _historyService.LogAsync(EntityType.Order, order.id.ToString(), "Created", $"Order {order.reference} created.");
        }

        public async Task DeleteOrder(int id)
        {
            var order = await GetOrderById(id);
            if (order == null) throw new Exception("Order not found!");

            foreach (var item in order.items)
            {
                await _inventoryLocationService.RemoveOrder(item.item_id, item.amount);
            }

            _Db.Order.Remove(order);
            await _Db.SaveChangesAsync();
            await _historyService.LogAsync(EntityType.Order, id.ToString(), "Deleted", $"Order {order.reference} deleted.");
        }
    }
}