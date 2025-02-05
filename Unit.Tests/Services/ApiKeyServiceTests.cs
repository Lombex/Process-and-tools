using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpAPI.Data;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Unit.Tests.Services
{
    public class ApiKeyServiceTests : IDisposable
    {
        private readonly SQLiteDatabase _db;
        private readonly IApiKeyService _service;
        private readonly IAuthService _authService;

        public ApiKeyServiceTests()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;
                
            _db = new SQLiteDatabase(options);
            _authService = new AuthService(_db, null);
            _service = new ApiKeyService(_db, _authService);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetAllApiKeys_ReturnsAllKeys_WhenUserIsAdmin()
        {
            // Arrange
            var currentUser = new ApiUser { role = "Admin", api_key = "admin_key", app = "TestApp" };
            var apiKeys = new List<ApiUser>
            {
                new ApiUser { id = 1, role = "Admin", warehouse_id = 1, api_key = "key1", app = "App1" },
                new ApiUser { id = 2, role = "User", warehouse_id = 1, api_key = "key2", app = "App2" }
            };
            await _db.ApiUsers.AddRangeAsync(apiKeys);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAllApiKeys(currentUser);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, k => k.role == "Admin");
            Assert.Contains(result, k => k.role == "User");
        }

        [Fact]
        public async Task GetApiKeyById_ReturnsKey_WhenUserIsAdmin()
        {
            // Arrange
            var currentUser = new ApiUser { role = "Admin", api_key = "admin_key", app = "TestApp" };
            var apiKey = new ApiUser { id = 1, role = "User", warehouse_id = 1, api_key = "test_key", app = "TestApp" };
            await _db.ApiUsers.AddAsync(apiKey);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetApiKeyById(1, currentUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(apiKey.id, result.id);
        }

        [Fact]
        public async Task AddApiKey_AddsNewKey()
        {
            // Arrange
            var newKey = new ApiUser 
            { 
                role = "User",
                warehouse_id = 1,
                app = "TestApp",
                api_key = "new_test_key"
            };

            // Act
            await _service.AddApiKey(newKey);

            // Assert
            var savedKey = await _db.ApiUsers.FirstOrDefaultAsync(k => k.app == "TestApp");
            Assert.NotNull(savedKey);
            Assert.Equal(newKey.role, savedKey.role);
            Assert.Equal(newKey.warehouse_id, savedKey.warehouse_id);
        }

        [Fact]
        public async Task UpdateApiKey_UpdatesExistingKey()
        {
            // Arrange
            var existingKey = new ApiUser 
            { 
                id = 1,
                role = "User",
                warehouse_id = 1,
                app = "OldApp",
                api_key = "old_key"
            };
            await _db.ApiUsers.AddAsync(existingKey);
            await _db.SaveChangesAsync();

            var updatedKey = new ApiUser
            {
                role = "Admin",
                warehouse_id = 2,
                app = "NewApp",
                api_key = "updated_key"
            };

            // Act
            await _service.UpdateApiKey(1, updatedKey);

            // Assert
            var result = await _db.ApiUsers.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("NewApp", result.app);
            Assert.Equal("Admin", result.role);
            Assert.Equal(2, result.warehouse_id);
        }

        [Fact]
        public async Task DeleteApiKey_RemovesKey()
        {
            // Arrange
            var apiKey = new ApiUser 
            { 
                id = 1, 
                role = "User", 
                warehouse_id = 1, 
                api_key = "key_to_delete",
                app = "TestApp"
            };
            await _db.ApiUsers.AddAsync(apiKey);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteApiKey(1);

            // Assert
            var result = await _db.ApiUsers.FindAsync(1);
            Assert.Null(result);
        }
    }
}
