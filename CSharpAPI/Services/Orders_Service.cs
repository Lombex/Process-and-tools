using CSharpAPI.Models;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface IOrderService
    {
        List<OrdersModel> GetAllOrders();
        OrdersModel GetOrderById(int id);
        List<Items> GetItemByOrderId(int id);
        bool UpdateOrders(int id, OrdersModel updatedOrders);
        void CreateOrder(OrdersModel orders);
        bool DeleteOrder(int id);
    }
    public class OrderService
    {
        private readonly string dummydata = "data/transfer.json";

        public List<OrdersModel> GetAllOrders()
        {
            if (!File.Exists(dummydata)) return new List<OrdersModel>();
            return JsonConvert.DeserializeObject<List<OrdersModel>>(File.ReadAllText(dummydata)) ?? new List<OrdersModel>();

            // still returns our dummy data
        }

        public OrdersModel GetOrderById(int id)
        {
            var _order = GetAllOrders().FirstOrDefault(x => x.id == id);
            if (_order == null) throw new Exception("This Order does not exits!");
            return _order;
        }

        public List<Items> GetItemByOrderId(int id)
        {
            var order = GetAllOrders().FirstOrDefault(x => x.id == id);
            if (order == null) throw new Exception("This order does not exists!");
            if (order.items == null) return new List<Items>();
            return order.items;
        }

        public bool UpdateOrders(int id, OrdersModel updatedOrders)
        {
            var _order = GetAllOrders().FirstOrDefault(x => x.id == id);
            if (_order == null) return false;

            updatedOrders.id = _order.id;
            updatedOrders.source_id = _order.source_id;
            updatedOrders.order_date = _order.order_date;
            updatedOrders.request_date = _order.request_date;
            updatedOrders.reference = _order.reference;
            updatedOrders.reference_extra = _order.reference_extra;
            updatedOrders.order_status = _order.order_status;
            updatedOrders.notes = _order.notes;
            updatedOrders.shipping_notes = _order.shipping_notes;
            updatedOrders.picking_notes = _order.picking_notes;
            updatedOrders.warehouse_id = _order.warehouse_id;
            updatedOrders.ship_to = _order.ship_to;
            updatedOrders.bill_to = _order.bill_to;
            updatedOrders.shipment_id = _order.shipment_id;
            updatedOrders.total_amount = _order.total_amount;
            updatedOrders.total_discount = _order.total_discount;
            updatedOrders.total_tax = _order.total_tax;
            updatedOrders.total_surcharge = _order.total_surcharge;
            updatedOrders.updated_at = DateTime.Now;
            updatedOrders.items = _order.items;

            // Update Database here.

            return true;
        }

        public void CreateOrders(OrdersModel orders)
        {
            var AllOrders = GetAllOrders();
            orders.id = AllOrders.Count > 0 ? AllOrders.Max(x => x.id) + 1 : 1;
            AllOrders.Add(orders);

            // Update Database here.
        }

        public bool DeleteOrders(int id)
        {
            var _order = GetAllOrders().FirstOrDefault(x => x.id == id);
            if (_order == null) return false;

            // Update Database here.
            return true;
        }
    }
}