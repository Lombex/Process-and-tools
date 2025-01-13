namespace CSharpAPI.Models
{
    public enum EntityType
    {
        Item,
        Order,
        Shipment
    }

    public class History
    {
        public int Id { get; set; }
        public EntityType EntityType { get; set; } // Geeft het type entiteit aan
        public string EntityId { get; set; } // Het ID van de entiteit (bijv. Item ID)
        public string Action { get; set; } // 'Created', 'Updated', 'Deleted'
        public string Changes { get; set; } // Details van de wijzigingen
        // public int? UserId { get; set; } // Optioneel: wie de actie uitvoerde
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Tijdstip van de actie
    }
}