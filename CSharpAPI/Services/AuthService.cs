using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CSharpAPI.Data;
using CSharpAPI.Models.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CSharpAPI.Services.Auth
{
    public interface IAuthService
    {
        Task<ApiUser> GetUserByApiKey(string apiKey);
        Task<bool> HasAccess(ApiUser user, string resource, string method);
        string GenerateJwtToken(ApiUser user);
        Task EnsureRolePermissions(string role);
    }

    public class AuthService : IAuthService
    {
        private readonly SQLiteDatabase _db;
        private readonly IConfiguration _configuration;

        public AuthService(SQLiteDatabase db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<ApiUser> GetUserByApiKey(string apiKey)
        {
            return await _db.ApiUsers.FirstOrDefaultAsync(u => u.api_key == apiKey);
        }

        public async Task<bool> HasAccess(ApiUser user, string resource, string method)
        {
            if (user == null) return false;

            if (user.role == "Admin") return true;

            var permission = await _db.RolePermissions
                .FirstOrDefaultAsync(p => p.role == user.role && p.resource == resource.ToLower());

            if (permission == null) return false;

            return method.ToUpper() switch
            {
                "GET" => permission.can_view,
                "POST" => permission.can_create,
                "PUT" => permission.can_update,
                "DELETE" => permission.can_delete,
                _ => false
            };
        }

        public string GenerateJwtToken(ApiUser user)
        {
            // Get the JWT configuration from appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Add claims to the token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.api_key),
                new Claim("role", user.role),
                new Claim("app", user.app),
            };

            // Create the token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task EnsureRolePermissions(string role)
        {
            // Define default permissions for different roles
            var defaultPermissions = new Dictionary<string, List<(string resource, bool view, bool create, bool update, bool delete)>>
            {
                ["Admin"] = new List<(string, bool, bool, bool, bool)>
                {
                    ("warehouses", true, true, true, true),
                    ("items", true, true, true, true),
                    ("clients", true, true, true, true),
                    ("orders", true, true, true, true),
                    ("inventories", true, true, true, true),
                    ("shipments", true, true, true, true),
                    ("suppliers", true, true, true, true),
                    ("locations", true, true, true, true),
                    ("transfers", true, true, true, true),
                    ("keys", true, true, true, true)
                },
                ["Warehouse_Manager"] = new List<(string, bool, bool, bool, bool)>
                {
                    ("warehouses", true, false, false, false),
                    ("locations", true, false, false, false),
                    ("transfers", true, false, false, false)
                },
                ["Inventory_Manager"] = new List<(string, bool, bool, bool, bool)>
                {
                    ("inventories", true, true, true, false),
                    ("items", true, false, false, false),
                    ("locations", true, false, false, false)
                },
                ["Floor_Manager"] = new List<(string, bool, bool, bool, bool)>
                {
                    ("inventories", true, false, false, false),
                    ("items", true, false, false, false),
                    ("locations", true, false, false, false)
                },
                ["Operative"] = new List<(string, bool, bool, bool, bool)>
                {
                    ("inventories", true, false, false, false),
                    ("items", true, false, false, false)
                },
                ["Supervisor"] = new List<(string, bool, bool, bool, bool)>
                {
                    ("warehouses", true, false, false, false),
                    ("inventories", true, false, false, false),
                    ("items", true, false, false, false),
                    ("orders", true, false, false, false)
                },
                ["Analyst"] = new List<(string, bool, bool, bool, bool)>
                {
                    ("warehouses", true, false, false, false),
                    ("inventories", true, false, false, false),
                    ("items", true, false, false, false),
                    ("orders", true, false, false, false),
                    ("clients", true, false, false, false),
                    ("suppliers", true, false, false, false)
                },
                ["Logistics"] = new List<(string, bool, bool, bool, bool)>
                {
                    ("warehouses", true, false, false, false),
                    ("transfers", true, true, false, false),
                    ("shipments", true, true, false, false),
                    ("orders", true, false, false, false)
                },
                ["Sales"] = new List<(string, bool, bool, bool, bool)>
                {
                    ("clients", true, true, true, false),
                    ("orders", true, true, false, false)
                }
            };

            // Check if permissions for this role already exist
            var existingPermissions = await _db.RolePermissions
                .Where(p => p.role == role)
                .ToListAsync();

            if (!existingPermissions.Any() && defaultPermissions.ContainsKey(role))
            {
                var newPermissions = defaultPermissions[role].Select(p => new RolePermission
                {
                    role = role,
                    resource = p.resource,
                    can_view = p.view,
                    can_create = p.create,
                    can_update = p.update,
                    can_delete = p.delete,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }).ToList();

                await _db.RolePermissions.AddRangeAsync(newPermissions);
                await _db.SaveChangesAsync();
            }
        }
    }
}