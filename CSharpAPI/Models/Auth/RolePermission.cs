using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models.Auth
{
    public class RolePermission
    {
        public int id { get; set; }
        public string role { get; set; }
        public string resource { get; set; }
        public bool can_view { get; set; }
        public bool can_create { get; set; }
        public bool can_update { get; set; }
        public bool can_delete { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}