namespace CSharpAPI.Models
{
    public class ArchivedShipmentModel
    {
        public int id { get; set; } // Unieke identifier van de zending
        public int source_id { get; set; } // Bron-ID voor tracking
        public string? order_date { get; set; } // Datum van de bestelling
        public string? request_date { get; set; } // Verzoekdatum
        public string? shipment_date { get; set; } // Datum van verzending
        public string? shipment_type { get; set; } // Type verzending
        public string? shipment_status { get; set; } // Status van de verzending
        public string? notes { get; set; } // Algemene notities
        public string? carrier_code { get; set; } // Code van de vervoerder
        public string? carrier_description { get; set; } // Beschrijving van de vervoerder
        public string? service_code { get; set; } // Servicecode
        public string? payment_type { get; set; } // Betalingstype
        public string? transfer_mode { get; set; } // Overdrachtsmodus
        public int total_package_count { get; set; } // Totaal aantal pakketten
        public float total_package_weight { get; set; } // Totaal gewicht van pakketten
        public DateTime created_at { get; set; } // Datum van aanmaak
        public DateTime updated_at { get; set; } // Datum van laatste update
        public DateTime archived_at { get; set; } // Datum van archivering
        public List<Items>? items { get; set; } // Lijst met items in de zending
    }
}
