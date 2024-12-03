namespace CSharpAPI.Models {
    public class InventorieModel {
        public int id { get; set; }
        public string? item_id { get; set; }
        public string? description { get; set; }
        public string? item_reference {get; set; }
        public List<int>? locations { get; set; }
        public int total_on_hand { get; set; }
        public int total_expected { get; set; }
        public int total_ordered { get; set; }
        public int total_allocated { get; set; }
        public int total_available { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class InventoriesTable
    {
        public readonly string inventorieQuery = @"
            CREATE TABLE IF NOT EXISTS Inventories (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            item_id TEXT,
            description TEXT,
            item_reference TEXT,
            locations TEXT, -- Stores locations as string (need to build logic for it.)
            total_on_hand INTEGER,
            total_expected INTEGER,
            total_ordered INTEGER,
            total_allocated INTEGER,
            total_available INTEGER,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
        )";
    }
}