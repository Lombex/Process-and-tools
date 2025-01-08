using CSharpAPI.Models.Auth;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using CSharpAPI.Controllers;

namespace CSharpAPI.Services.Auth
{
    public interface IApiKeyService
    {
        Task<List<ApiUser>> GetAllApiKeys(ApiUser currentUser);
        Task<ApiUser> GetApiKeyById(int id, ApiUser currentUser);
        Task AddApiKey(ApiUser apiUser);
        Task UpdateApiKey(int id, ApiUser apiUser);
        Task DeleteApiKey(int id);
        Task<ApiUser> GenerateApiKey(KeyGenerationRequest request);
    }

    public class ApiKeyService : IApiKeyService
    {
        private readonly SQLiteDatabase _db;

        public ApiKeyService(SQLiteDatabase db)
        {
            _db = db;
        }

        public async Task<List<ApiUser>> GetAllApiKeys(ApiUser currentUser)
        {
            var query = _db.ApiUsers.AsQueryable();

            // If not admin, only show keys for user's warehouse
            if (currentUser.role != "Admin" && currentUser.warehouse_id.HasValue)
            {
                query = query.Where(x => x.warehouse_id == currentUser.warehouse_id);
            }

            // Never show admin keys to non-admins
            if (currentUser.role != "Admin")
            {
                query = query.Where(x => x.role != "Admin");
            }

            return await query
                .OrderBy(x => x.role)
                .ThenBy(x => x.warehouse_id)
                .ToListAsync();
        }

        public async Task<ApiUser> GetApiKeyById(int id, ApiUser currentUser)
        {
            var apiKey = await _db.ApiUsers.FirstOrDefaultAsync(x => x.id == id);
            
            if (apiKey == null)
                return null;

            // Non-admins can't see admin keys
            if (currentUser.role != "Admin" && apiKey.role == "Admin")
                return null;

            // Non-admins can only see keys for their warehouse
            if (currentUser.role != "Admin" && currentUser.warehouse_id.HasValue 
                && apiKey.warehouse_id != currentUser.warehouse_id)
                return null;

            return apiKey;
        }

        public async Task AddApiKey(ApiUser apiUser)
        {
            if (apiUser == null)
                throw new ArgumentNullException(nameof(apiUser));

            apiUser.created_at = DateTime.UtcNow;
            apiUser.updated_at = DateTime.UtcNow;

            await _db.ApiUsers.AddAsync(apiUser);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateApiKey(int id, ApiUser apiUser)
        {
            var existingKey = await _db.ApiUsers.FirstOrDefaultAsync(x => x.id == id);
            if (existingKey == null)
                throw new Exception($"API key with id {id} not found");

            existingKey.app = apiUser.app;
            existingKey.role = apiUser.role;
            existingKey.warehouse_id = apiUser.warehouse_id;
            existingKey.updated_at = DateTime.UtcNow;

            _db.ApiUsers.Update(existingKey);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteApiKey(int id)
        {
            var apiKey = await _db.ApiUsers.FirstOrDefaultAsync(x => x.id == id);
            if (apiKey == null)
                throw new Exception($"API key with id {id} not found");

            _db.ApiUsers.Remove(apiKey);
            await _db.SaveChangesAsync();
        }

        public async Task<ApiUser> GenerateApiKey(KeyGenerationRequest request)
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var newKey = new ApiUser
            {
                api_key = Convert.ToBase64String(bytes),
                role = request.role,
                app = request.app,
                warehouse_id = request.warehouse_id,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            await _db.ApiUsers.AddAsync(newKey);
            await _db.SaveChangesAsync();

            return newKey;
        }

        private string GenerateKey()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }
    }
}