using System;
using System.Collections.Generic;

namespace CSharpAPI.Models
{
    public class Transfer
    {
        public int Id { get; set; }
        public string? Reference { get; set; }
        public int? TransferFrom { get; set; }  // Nullable to allow null values
        public int TransferTo { get; set; }
        public string? TransferStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }  // Nullable to allow null values
        public List<TransferItem>? Items { get; set; }
    }

    public class TransferItem
    {
        public string? ItemId { get; set; }
        public int Amount { get; set; }
    }
}