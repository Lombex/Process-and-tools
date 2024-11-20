namespace CSharpAPI.Models
{
    public class ItemLineModel
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public int itemgroup_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}