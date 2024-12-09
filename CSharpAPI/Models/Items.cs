using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models {
    public class ItemModel {
        [Key]
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

    public class Items {
        [Key]
        public string? item_id { get; set; }
        public int amount { get; set; }
    }

}