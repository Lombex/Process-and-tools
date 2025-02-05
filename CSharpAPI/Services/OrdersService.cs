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
            
            if (existingOrder == null)
                throw new Exception("Order not found!");

            // Update order details
            existingOrder.reference_extra = updatedOrder.reference_extra ?? existingOrder.reference_extra;
            existingOrder.reference = updatedOrder.reference ?? existingOrder.reference;
            existingOrder.order_status = updatedOrder.order_status ?? existingOrder.order_status;
            existingOrder.order_date = updatedOrder.order_date ?? existingOrder.order_date;
            existingOrder.request_date = updatedOrder.request_date ?? existingOrder.request_date;
            existingOrder.notes = updatedOrder.notes ?? existingOrder.notes;
            existingOrder.shipping_notes = updatedOrder.shipping_notes ?? existingOrder.shipping_notes;
            existingOrder.picking_notes = updatedOrder.picking_notes ?? existingOrder.picking_notes;
            existingOrder.warehouse_id = updatedOrder.warehouse_id != 0 ? updatedOrder.warehouse_id : existingOrder.warehouse_id;
            existingOrder.ship_to = updatedOrder.ship_to != 0 ? updatedOrder.ship_to : existingOrder.ship_to;
            existingOrder.bill_to = updatedOrder.bill_to != 0 ? updatedOrder.bill_to : existingOrder.bill_to;
            existingOrder.total_amount = updatedOrder.total_amount != 0 ? updatedOrder.total_amount : existingOrder.total_amount;
            existingOrder.total_discount = updatedOrder.total_discount != 0 ? updatedOrder.total_discount : existingOrder.total_discount;
            existingOrder.total_tax = updatedOrder.total_tax != 0 ? updatedOrder.total_tax : existingOrder.total_tax;
            existingOrder.total_surcharge = updatedOrder.total_surcharge != 0 ? updatedOrder.total_surcharge : existingOrder.total_surcharge;

            // Ensure order has items
            if (existingOrder.items == null)
                existingOrder.items = new List<Items>();

            if (updatedOrder.items != null && updatedOrder.items.Any())
            {
                foreach (var updatedItem in updatedOrder.items)
                {
                    var originalItem = existingOrder.items.FirstOrDefault(i => i.item_id == updatedItem.item_id);

                    if (originalItem != null)
                    {
                        int quantityChange = updatedItem.amount - originalItem.amount;

                        if (quantityChange > 0)
                        {
                            await _inventoryLocationService.PlaceOrder(updatedItem.item_id, quantityChange);
                        }
                        else if (quantityChange < 0)
                        {
                            await _inventoryLocationService.RemoveOrder(updatedItem.item_id, -quantityChange);
                        }

                        originalItem.amount = updatedItem.amount;
                    }
                    else
                    {
                        // If item doesn't exist, add it to order
                        existingOrder.items.Add(new Items { item_id = updatedItem.item_id, amount = updatedItem.amount });

                        // Update inventory for new item
                        await _inventoryLocationService.PlaceOrder(updatedItem.item_id, updatedItem.amount);
                    }
                }
            }

            existingOrder.updated_at = DateTime.UtcNow;

            _Db.Order.Update(existingOrder);
            await _Db.SaveChangesAsync();
            
            await _historyService.LogAsync(EntityType.Order, id.ToString(), "Updated", $"Order {existingOrder.reference} updated.");
        }



        public async Task CreateOrder(OrderModel order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            // Validate shipping and billing references
            if (!(await _Db.ClientModels.AnyAsync(client => client.id == order.ship_to)))
                throw new Exception($"Shipping address with Client ID {order.ship_to} does not exist.");

            if (!(await _Db.ClientModels.AnyAsync(client => client.id == order.bill_to)))
                throw new Exception($"Billing address with Client ID {order.bill_to} does not exist.");

            order.created_at = DateTime.UtcNow;
            order.updated_at = DateTime.UtcNow;

            await _Db.Order.AddAsync(order);
            await _Db.SaveChangesAsync();

            // Update inventory for each item in the order
            foreach (var item in order.items)
            {
                var inventory = await _Db.Inventors.FirstOrDefaultAsync(i => i.item_id == item.item_id);

                if (inventory == null)
                {
                    throw new Exception($"Inventory not found for item {item.item_id}");
                }

                // Update total_ordered
                inventory.total_ordered += item.amount;

                // Corrected Calculation for total_available
                inventory.total_available = inventory.total_on_hand - inventory.total_ordered;

                _Db.Inventors.Update(inventory);
            }

            await _Db.SaveChangesAsync();

            await _historyService.LogAsync(EntityType.Order, order.id.ToString(), "Created", $"Order {order.reference} created.");
        }


        public async Task DeleteOrder(int id)
        {
            var order = await GetOrderById(id);
            if (order == null) throw new Exception("Order not found!");

            if (order.items == null || !order.items.Any())
                throw new Exception("Order has no items!");

            var inventories = await _Db.Inventors
                .Where(i => order.items.Select(it => it.item_id).Contains(i.item_id))
                .ToListAsync();

            foreach (var inventory in inventories)
            {
                inventory.total_ordered -= order.items
                    .Where(it => it.item_id == inventory.item_id)
                    .Sum(it => it.amount);

                if (inventory.total_ordered < 0)
                    inventory.total_ordered = 0;

                inventory.total_allocated -= order.items
                    .Where(it => it.item_id == inventory.item_id)
                    .Sum(it => it.amount);

                if (inventory.total_allocated < 0)
                    inventory.total_allocated = 0;

                inventory.total_available = (inventory.total_on_hand + inventory.total_expected) 
                                        - (inventory.total_ordered + inventory.total_allocated);
            }

            _Db.Inventors.UpdateRange(inventories);

            var shipments = await _Db.Shipment
                .Where(s => s.id == order.shipment_id)
                .ToListAsync();

            if (shipments.Any())
            {
                _Db.Shipment.RemoveRange(shipments);
            }

            _Db.Order.Remove(order);
            await _Db.SaveChangesAsync();

            await _historyService.LogAsync(EntityType.Order, id.ToString(), "Deleted", $"Order {order.reference} and linked shipments deleted.");
        }


    }
}