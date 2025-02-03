using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSharpAPI.Models
{
    [Table("inventory_locations")]
    public class InventoryLocationModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("inventory_id")]
        public int InventoryId { get; set; }

        [Required]
        [Column("location_id")]
        public int LocationId { get; set; }

        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties without [Required]
        [ForeignKey("InventoryId")]
        public virtual InventorieModel? Inventory { get; set; }

        [ForeignKey("LocationId")]
        public virtual LocationModel? Location { get; set; }
    }
}