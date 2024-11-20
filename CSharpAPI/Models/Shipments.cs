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
}