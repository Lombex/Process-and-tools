namespace CSharpAPI.Models {
    public class SupplierModel{
        public int id {get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? address { get; set; }
        public string? address_extra { get; set; }
        public string? city { get; set; }
        public string? zip_code { get; set; }
        public string? province { get; set; }
        public string? contact_name { get; set; }
        public string? phonenumber { get; set; }
        public string? reference { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}