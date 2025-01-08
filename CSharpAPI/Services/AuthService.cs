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
    }
}
