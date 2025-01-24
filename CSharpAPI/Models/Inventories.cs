using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Models {
    public class InventorieModel {
        public int id { get; set; }
        public string? item_id { get; set; }
        public string? description { get; set; }
        public string? item_reference {get; set; }
        public List<AmountPerLocation>? locations { get; set; }
        public int total_on_hand { get; set; }
        public int total_expected { get; set; }
        public int total_ordered { get; set; }
        public int total_allocated { get; set; }
        public int total_available { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    [Owned]
    public class AmountPerLocation
    {
        private int _amount = 0;
        public int location_id { get; set; }
        public int amount 
        { 
            get => _amount;
            set => _amount = value > 0 ? value : 0; 
        }
    }
}