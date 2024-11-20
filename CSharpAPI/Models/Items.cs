namespace CSharpAPI.Models {
    public class ItemsModel {
        public string? uid { get; set; }
        public string? code { get; set; }
        public string? description { get; set; }
        public string? short_description { get; set; }
        public string? upc_code { get; set; }
        public string? model_number { get; set; }
        public string? commodity_code { get; set; }
        public int item_line { get; set; }
        public int item_group { get; set; }
        public int item_type { get; set; }
        public int unit_purchase_quantity { get; set; }
        public int unit_order_quantity { get; set; }
        public int pack_order_quantity { get; set; }
        public int supplier_id { get; set; }
        public string? supplier_code { get; set; }
        public string? supplier_part_number { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class ItemTable 
    {
        public readonly string itemsQuery = @" 
            CREATE TABLE IF NOT EXISTS Items (
            uid TEXT PRIMARY KEY,
            code TEXT,
            description TEXT,
            short_description TEXT,
            upc_code TEXT,
            model_number TEXT,
            commodity_code TEXT,
            item_line INTEGER,
            item_group INTEGER,
            item_type INTEGER,
            unit_order_quantity INTEGER,
            pack_order_quantity INTEGER,
            supplier_id INTEGER,
            supplier_code TEXT,
            supplier_part_number TEXT,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
        )";
    }

    public class Items { 
        public string? item_id { get; set; }
        public int amount { get; set; }
    }
    /// <summary>
    /// This model is for Groups Lines and Types
    /// </summary>
    public class Item_XModel {
        public int id { get; set; }
        public string? name { get; set; }
        public string? description {get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
    public class XModelTable() 
    {
        public readonly Dictionary<string, string> xModelTableQuery = new Dictionary<string, string>
        {
            {  
                "ItemTypes", 
                @"CREATE TABLE IF NOT EXISTS ItemTypes (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT,
                description TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP)"
            },
            {
                "ItemGroups",
                @"CREATE TABLE IF NOT EXISTS ItemGroups (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT,
                description TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP)"
            },
            {
                "ItemLines",
                @"CREATE TABLE IF NOT EXISTS ItemLines (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT,
                description TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP)"
            }
        };
    }
}