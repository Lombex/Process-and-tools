namespace CSharpAPI.Models
{
    public class OrderShipmentMapping
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ShipmentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}