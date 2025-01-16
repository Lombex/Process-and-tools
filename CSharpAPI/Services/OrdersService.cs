using CSharpAPI.Data;
using CSharpAPI.Models;
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
    }
    public class OrderService : IOrderService
    {
        private readonly SQLiteDatabase _Db;
        public OrderService(SQLiteDatabase sQLite) 
        {
            _Db = sQLite;
        }
        public async Task<List<OrderModel>> GetAllOrders() => await _Db.Order.AsQueryable().ToListAsync(); 
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
        public async Task UpdateOrders(int id, OrderModel updatedOrders)
        {
            var _order = await GetOrderById(id);

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
            _order.shipment_id = updatedOrders.shipment_id;
            _order.total_amount = updatedOrders.total_amount;
            _order.total_discount = updatedOrders.total_discount;
            _order.total_tax = updatedOrders.total_tax;
            _order.total_surcharge = updatedOrders.total_surcharge;
            _order.updated_at = DateTime.Now;
            _order.items = updatedOrders.items;

            _Db.Order.Update(_order);
            await _Db.SaveChangesAsync();
        }
        public async Task CreateOrder(OrderModel orders)
        {
            if (orders == null) throw new ArgumentNullException(nameof(orders));

            var shipToExists = await _Db.ClientModels.AnyAsync(client => client.id == orders.ship_to);
            if (!shipToExists) throw new Exception($"Shipping address with Client ID {orders.ship_to} does not exist.");

            var billToExists = await _Db.ClientModels.AnyAsync(client => client.id == orders.bill_to);
            if (!billToExists) throw new Exception($"Billing address with Client ID {orders.bill_to} does not exist.");

            orders.created_at = DateTime.UtcNow;
            orders.updated_at = DateTime.UtcNow;

            await _Db.Order.AddAsync(orders);
            await _Db.SaveChangesAsync();
        }


        public async Task DeleteOrder(int id)
        {
            var _order = await GetOrderById(id);
            _Db.Order.Remove(_order);
            await _Db.SaveChangesAsync();
        }
    }
}