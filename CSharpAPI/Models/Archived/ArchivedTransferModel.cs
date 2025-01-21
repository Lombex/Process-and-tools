namespace CSharpAPI.Models
{
    public class ArchivedTransferModel
    {
        public int id { get; set; }
        public string? reference { get; set; }
        public int? transfer_from { get; set; }
        public int transfer_to { get; set; }
        public string? transfer_status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime archived_at { get; set; }
    }
}
