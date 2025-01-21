namespace CSharpAPI.Models
{
    public class DockModel
    {
        public int? id { get; set; } = null;
        public int warehouse_id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}