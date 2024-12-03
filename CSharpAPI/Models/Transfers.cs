using System;
using System.Collections.Generic;

namespace CSharpAPI.Models
{
    public class TransferModel
    {
        public int id { get; set; }
        public string? reference { get; set; }
        public int? transfer_from { get; set; } 
        public int transfer_to { get; set; }
        public string? transfer_status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; } 
        public List<Items>? items { get; set; }
    }

    public class TransferTable
    {
        public readonly Dictionary<string, string> transferQuery = new Dictionary<string, string>
        {
            {
                "Transfers",
                @"CREATE TABLE IF NOT EXISTS Transfers (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    reference TEXT,
                    transfer_from INTEGER,
                    transfer_to INTEGER,
                    transfer_status TEXT,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
                )"
            },
            {
                "TransferItems",
                @"CREATE TABLE IF NOT EXISTS TransferItems (
                    item_id TEXT PRIMARY KEY,
                    amount INTEGER
                )"
            }
        };
    }
}