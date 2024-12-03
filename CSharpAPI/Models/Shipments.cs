namespace CSharpAPI.Models {

    public class ShipmentModel { 
        public int id { get; set; }
        public int order_id { get; set ;}
        public int source_id { get; set; }
        public string? order_date { get; set; }
        public string? request_date { get; set; }
        public string? shipment_date { get; set; }
        public string? shipment_type { get; set; }
        public string? shipment_status { get; set; }
        public string? notes { get; set; }
        public string? carrier_code { get; set; }
        public string? carrier_description { get; set; }
        public string? service_code { get; set; }
        public string? payment_type { get; set; }
        public string? transfer_mode { get; set; }
        public int total_package_count { get; set; }
        public float total_package_weight { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public List<Items>? items { get; set; }
    }

    public class ShipmentTable
    {
        public readonly Dictionary<string, string> shipmentQuery = new Dictionary<string, string>()
        {
            {
                "Shipment",
                @"CREATE TABLE IF NOT EXISTS Shipments (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    order_id INTEGER,
                    source_id INTEGER,
                    order_date TEXT,
                    request_date TEXT,
                    shipment_date TEXT,
                    shipment_type TEXT,
                    shipment_status TEXT,
                    notes TEXT,
                    carrier_code TEXT,
                    carrier_description TEXT,
                    service_code TEXT,
                    payment_type TEXT,
                    transfer_mode TEXT,
                    total_package_count INTEGER,
                    total_package_weight REAL,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
                )"
            },
            {
                "ShipmentItem",
                @"CREATE TABLE IF NOT EXISTS OrderItems (
                    item_id TEXT PRIMARY KEY,
                    amount INTEGER
                )"
            }
        };
    }
}