namespace CSharpAPI.Models
{
    public class ArchivedOrderModel
    {
        public int id { get; set; } // Unieke identifier van de order
        public int source_id { get; set; } // Bron-ID voor tracking
        public string? order_date { get; set; } // Datum van de bestelling
        public string? request_date { get; set; } // Datum van het verzoek
        public string? reference { get; set; } // Referentie van de order
        public string? reference_extra { get; set; } // Extra referentie
        public string? order_status { get; set; } // Status van de order
        public string? notes { get; set; } // Algemene notities
        public string? shipping_notes { get; set; } // Verzendnotities
        public string? picking_notes { get; set; } // Picking notities
        public int warehouse_id { get; set; } // ID van het magazijn
        public int ship_to { get; set; } // ID van de ontvanger
        public int bill_to { get; set; } // ID van de factuurontvanger
        public float total_amount { get; set; } // Totaalbedrag van de order
        public float total_discount { get; set; } // Totaalkorting
        public float total_tax { get; set; } // Totale belasting
        public float total_surcharge { get; set; } // Totale toeslag
        public DateTime created_at { get; set; } // Datum van aanmaak
        public DateTime updated_at { get; set; } // Datum van laatste update
        public DateTime archived_at { get; set; } // Datum van archivering
        public List<Items>? items { get; set; } // Lijst met items in de order
    }
}