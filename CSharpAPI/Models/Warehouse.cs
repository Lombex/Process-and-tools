
namespace CSharpAPI.Models
{
    public class WarehouseModel
    {
        public int id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? address { get; set; }
        public string? zip { get; set; }
        public string? city { get; set; }
        public string? province { get; set; }
        public string? country { get; set; }
        public Contact? contact { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Contact
    {
        public string? name { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
    }

    public class WarehouseTable
    {
        public readonly Dictionary<string, string> warhouseQuery = new Dictionary<string, string>()
        {
            {
                "Warehouse",
                @"CREATE TABLE IF NOT EXISTS Warehouse (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    code TEXT,
                    name TEXT,
                    address TEXT,
                    zip TEXT,
                    city TEXT,
                    province TEXT,
                    country TEXT,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
                )"
            },
            {
                "WarehouseContact",
                @"CREATE TABLE IF NOT EXISTS Contact (
                    name TEXT,
                    phone TEXT,
                    email TEXT
                )"
            }
        };
    }
}
