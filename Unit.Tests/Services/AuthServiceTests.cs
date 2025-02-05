
using System;
using System.Threading.Tasks;
using CSharpAPI.Data;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Unit.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly SQLiteDatabase _db;
        private readonly IAuthService _service;
        private readonly IConfiguration _configuration;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Jwt:Key", "YourTestSecretKeyHere12345678901234567890"},
                    {"Jwt:Issuer", "TestIssuer"},
                    {"Jwt:Audience", "TestAudience"}
                })
                .Build();
                
            _db = new SQLiteDatabase(options);
            _service = new AuthService(_db, _configuration);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetUserByApiKey_ReturnsUser_WhenKeyExists()
        {
            // Arrange
            var apiUser = new ApiUser 
            { 
                api_key = "test_key",
                role = "Admin",
                app = "TestApp"
            };
            await _db.ApiUsers.AddAsync(apiUser);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetUserByApiKey("test_key");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(apiUser.api_key, result.api_key);
            Assert.Equal(apiUser.role, result.role);
        }

        [Fact]
        public async Task GetUserByApiKey_ReturnsNull_WhenKeyDoesNotExist()
        {
            // Act
            var result = await _service.GetUserByApiKey("nonexistent_key");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("Admin", true)]
        [InlineData("User", false)]
        public async Task HasAccess_AdminHasFullAccess(string role, bool expectedResult)
        {
            // Arrange
            var user = new ApiUser { role = role, app = "TestApp", api_key = "test_key" };

            // Act
            var result = await _service.HasAccess(user, "any_resource", "GET");

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("GET", true, false, false, false, true)]
        [InlineData("POST", false, true, false, false, true)]
        [InlineData("PUT", false, false, true, false, true)]
        [InlineData("DELETE", false, false, false, true, true)]
        public async Task HasAccess_ChecksCorrectPermissions(string method, bool canView, bool canCreate, bool canUpdate, bool canDelete, bool expectedResult)
        {
            // Arrange
            var user = new ApiUser { role = "User", app = "TestApp", api_key = "test_key" };
            var permission = new RolePermission
            {
                role = "User",
                resource = "test_resource",
                can_view = canView,
                can_create = canCreate,
                can_update = canUpdate,
                can_delete = canDelete
            };
            await _db.RolePermissions.AddAsync(permission);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.HasAccess(user, "test_resource", method);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GenerateJwtToken_CreatesValidToken()
        {
            // Arrange
            var user = new ApiUser 
            { 
                api_key = "test_key",
                role = "Admin",
                app = "TestApp"
            };

            // Act
            var token = _service.GenerateJwtToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Warehouse_Manager")]
        [InlineData("Inventory_Manager")]
        public async Task EnsureRolePermissions_CreatesPermissions_WhenNotExist(string role)
        {
            // Act
            await _service.EnsureRolePermissions(role);

            // Assert
            var permissions = await _db.RolePermissions.Where(p => p.role == role).ToListAsync();
            Assert.NotEmpty(permissions);
            Assert.All(permissions, p => Assert.Equal(role, p.role));
        }

        [Fact]
        public async Task EnsureRolePermissions_DoesNotDuplicate_WhenPermissionsExist()
        {
            // Arrange
            var existingPermission = new RolePermission
            {
                role = "Admin",
                resource = "test_resource",
                can_view = true,
                can_create = true,
                can_update = true,
                can_delete = true
            };
            await _db.RolePermissions.AddAsync(existingPermission);
            await _db.SaveChangesAsync();

            // Act
            await _service.EnsureRolePermissions("Admin");

            // Assert
            var permissions = await _db.RolePermissions
                .Where(p => p.role == "Admin" && p.resource == "test_resource")
                .ToListAsync();
            Assert.Single(permissions);
        }
    }
}
