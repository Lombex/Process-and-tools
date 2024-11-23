namespace CSharpAPI.Models {
    public class ClientModel {
        public int id { get; set; }
        public string? name{ get; set; }
        public string? address{ get; set; }
        public string? city { get; set; }
        public string? zip_code { get; set; }
        public string? province { get; set; }
        public string? country { get; set; }
        public string? contact_name { get; set; }
        public string? contact_phone { get; set; }
        public string? contact_email { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class ClientTable
    {
        public readonly string clientQuery = 
            @"CREATE TABLE IF NOT EXISTS Client (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT,
                address TEXT,
                city TEXT,
                zip_code TEXT,
                province TEXT,
                country TEXT,
                contact_name TEXT,
                contact_phone TEXT,
                contact_email TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )";
    }
}