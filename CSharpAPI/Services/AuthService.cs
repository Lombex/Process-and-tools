using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services.Auth
{

    public interface IAuthService
    {
        Task<ApiUser> GetUserByApiKey(string apiKey);
        Task<bool> HasAccess(ApiUser user, string resource, string method);
    }
    public class AuthService : IAuthService
    {
        private readonly SQLiteDatabase _db;

        public AuthService(SQLiteDatabase db)
        {
            _db = db;
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
    }
}

