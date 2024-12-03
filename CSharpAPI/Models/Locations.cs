namespace CSharpAPI.Models 
{
    public class LocationModel
    {
        public int id { get; set; }
        public int warehouse_id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class LocationTable
    {
        public readonly string locationQuery =
            @"CREATE TABLE IF NOT EXISTS Location (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            warehouse_id INTEGER,
            code TEXT,
            name TEXT,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )";
    }
}