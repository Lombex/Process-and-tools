namespace CSharpAPI.Models 
{
    public class OrderModel 
    {
        public int id { get; set; }
        public int source_id { get; set; }
        public string? order_date { get; set; }
        public string? request_date { get; set; }
        public string? reference { get; set; }
        public string? reference_extra { get; set; }
        public string? order_status { get; set; }
        public string? notes { get; set; }
        public string? shipping_notes { get; set; }
        public string? picking_notes { get; set; }
        public int warehouse_id { get; set; }
        public int ship_to { get; set; }
        public int bill_to { get; set; }
        public int shipment_id { get; set; }
        public float total_amount { get; set; }
        public float total_discount { get; set; }
        public float total_tax { get; set; }
        public float total_surcharge { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public List<Items>? items { get; set; }
    }

    public class OrderTable
    {
        public readonly Dictionary<string, string> orderQuery = new Dictionary<string, string>
        {
            {
                "Order",
                @"CREATE TABLE IF NOT EXISTS OrderTable (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    source_id INTEGER,
                    order_date TEXT,
                    request_date TEXT,
                    reference TEXT,
                    reference_extra TEXT,
                    order_status TEXT,
                    notes TEXT,
                    shipping_notes TEXT,
                    picking_notes TEXT,
                    warehouse_id INTEGER,
                    ship_to INTEGER,
                    bill_to INTEGER,
                    shipment_id INTEGER,
                    total_amount REAL,
                    total_discount REAL,
                    total_tax REAL,
                    total_surcharge REAL,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
                )"
            },
            {
                "OrderItem",
                @"CREATE TABLE IF NOT EXISTS OrderItems (
                    item_id TEXT PRIMARY KEY,
                    amount INTEGER
                )"
            }
        };
    }
}