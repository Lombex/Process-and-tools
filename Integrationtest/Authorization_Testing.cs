using CSharpAPI.Data;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using CSharpAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Integration_Testing
{
    public class AuthorizationTests
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed API Users
            context.ApiUsers.AddRange(
                new ApiUser
                {
                    id = 1,
                    api_key = "admin_key_2024",
                    app = "Admin App",
                    role = "Admin",
                    warehouse_id = null,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser
                {
                    id = 2,
                    api_key = "warehouse_key_2024",
                    app = "Warehouse App",
                    role = "Warehouse Manager",
                    warehouse_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser
                {
                    id = 3,
                    api_key = "viewer_key_2024",
                    app = "Viewer App",
                    role = "Viewer",
                    warehouse_id = null,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            );

            // Seed Role Permissions
            context.RolePermissions.AddRange(
                new RolePermission
                {
                    id = 1,
                    role = "Admin",
                    resource = "warehouses",
                    can_view = true,
                    can_create = true,
                    can_update = true,
                    can_delete = true,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new RolePermission
                {
                    id = 2,
                    role = "Warehouse Manager",
                    resource = "warehouses",
                    can_view = true,
                    can_create = false,
                    can_update = true,
                    can_delete = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new RolePermission
                {
                    id = 3,
                    role = "Viewer",
                    resource = "warehouses",
                    can_view = true,
                    can_create = false,
                    can_update = false,
                    can_delete = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            );

            context.SaveChanges();
            return context;
        }

        private HttpContext CreateHttpContext(string apiKey = null)
        {
            var context = new DefaultHttpContext();
            if (apiKey != null)
            {
                context.Request.Headers["API_KEY"] = apiKey;
            }
            return context;
        }

        [Fact]
        public async Task AuthService_GetUserByApiKey_ReturnsCorrectUser()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var authService = new AuthService(dbContext);

            // Act
            var user = await authService.GetUserByApiKey("admin_key_2024");

            // Assert
            Assert.NotNull(user);
            Assert.Equal("Admin", user.role);
        }

        [Fact]
        public async Task AuthService_HasAccess_AdminHasFullAccess()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var authService = new AuthService(dbContext);
            var adminUser = await dbContext.ApiUsers.FirstAsync(u => u.role == "Admin");

            // Act
            var canView = await authService.HasAccess(adminUser, "warehouses", "GET");
            var canCreate = await authService.HasAccess(adminUser, "warehouses", "POST");
            var canUpdate = await authService.HasAccess(adminUser, "warehouses", "PUT");
            var canDelete = await authService.HasAccess(adminUser, "warehouses", "DELETE");

            // Assert
            Assert.True(canView);
            Assert.True(canCreate);
            Assert.True(canUpdate);
            Assert.True(canDelete);
        }

        [Fact]
        public async Task AuthService_HasAccess_ViewerHasLimitedAccess()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var authService = new AuthService(dbContext);
            var viewerUser = await dbContext.ApiUsers.FirstAsync(u => u.role == "Viewer");

            // Act
            var canView = await authService.HasAccess(viewerUser, "warehouses", "GET");
            var canCreate = await authService.HasAccess(viewerUser, "warehouses", "POST");

            // Assert
            Assert.True(canView);
            Assert.False(canCreate);
        }

        [Fact]
        public async Task AuthMiddleware_NoApiKey_Returns401()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var authService = new AuthService(dbContext);
            var context = CreateHttpContext();
            var middleware = new AuthMiddleware(next: (innerHttpContext) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context, authService);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task AuthMiddleware_InvalidApiKey_Returns401()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var authService = new AuthService(dbContext);
            var context = CreateHttpContext("invalid_key");
            var middleware = new AuthMiddleware(next: (innerHttpContext) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context, authService);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task AuthMiddleware_ValidApiKey_SetsUser()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var authService = new AuthService(dbContext);
            var context = CreateHttpContext("admin_key_2024");
            context.Request.Path = "/api/v1/warehouses";
            context.Request.Method = "GET";
            
            var middleware = new AuthMiddleware(next: (innerHttpContext) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context, authService);

            // Assert
            Assert.NotNull(context.Items["User"]);
            var user = context.Items["User"] as ApiUser;
            Assert.Equal("Admin", user.role);
        }

        [Fact]
        public async Task AuthMiddleware_UnauthorizedAccess_Returns403()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var authService = new AuthService(dbContext);
            var context = CreateHttpContext("viewer_key_2024");
            context.Request.Path = "/api/v1/warehouses";
            context.Request.Method = "POST";
            
            var middleware = new AuthMiddleware(next: (innerHttpContext) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context, authService);

            // Assert
            Assert.Equal(403, context.Response.StatusCode);
        }

        [Theory]
        [InlineData("GET", true)]
        [InlineData("POST", false)]
        [InlineData("PUT", true)]
        [InlineData("DELETE", false)]
        public async Task AuthService_WarehouseManager_HasCorrectPermissions(string method, bool expectedAccess)
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var authService = new AuthService(dbContext);
            var warehouseManager = await dbContext.ApiUsers.FirstAsync(u => u.role == "Warehouse Manager");

            // Act
            var hasAccess = await authService.HasAccess(warehouseManager, "warehouses", method);

            // Assert
            Assert.Equal(expectedAccess, hasAccess);
        }

        [Fact]
        public async Task AuthMiddleware_ResourceNotFound_Returns403()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var authService = new AuthService(dbContext);
            var context = CreateHttpContext("admin_key_2024");
            context.Request.Path = "/api/v1/nonexistent";
            context.Request.Method = "GET";
            
            var middleware = new AuthMiddleware(next: (innerHttpContext) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context, authService);

            // Assert
            Assert.Equal(403, context.Response.StatusCode);
        }

        [Fact]
        public async Task AuthService_InvalidMethod_ReturnsFalse()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var authService = new AuthService(dbContext);
            var adminUser = await dbContext.ApiUsers.FirstAsync(u => u.role == "Admin");

            // Act
            var hasAccess = await authService.HasAccess(adminUser, "warehouses", "INVALID");

            // Assert
            Assert.False(hasAccess);
        }

        [Fact]
        public async Task AuthMiddleware_InvalidPath_Returns403()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var authService = new AuthService(dbContext);
            var context = CreateHttpContext("admin_key_2024");
            context.Request.Path = "/invalid/path";
            context.Request.Method = "GET";
            
            var middleware = new AuthMiddleware(next: (innerHttpContext) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context, authService);

            // Assert
            Assert.Equal(403, context.Response.StatusCode);
        }
    }
}