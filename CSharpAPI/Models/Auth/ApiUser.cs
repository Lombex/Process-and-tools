using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models.Auth
{
    public class ApiUser
    {
        public int id { get; set; }
        public string api_key { get; set; }
        public string app { get; set; }
        public string role { get; set; }
        public int? warehouse_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}