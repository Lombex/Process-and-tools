using CSharpAPI.Data;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Integration.Tests.Infrastructure
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly SQLiteDatabase DbContext;
        protected readonly IConfiguration Configuration;
        protected readonly HistoryService HistoryService;

        protected readonly string AdminApiKey = "admin_test_key";
        protected readonly string WarehouseManagerApiKey = "wh_manager_test_key";
        protected readonly string InventoryManagerApiKey = "inv_manager_test_key";
        protected readonly string ViewerApiKey = "viewer_test_key";

        protected IntegrationTestBase()
        {
            // Create database options
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Initialize DbContext
            DbContext = new SQLiteDatabase(options);

            // Create test configuration
            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Jwt:Key", "YourSuperSecretTestKey"},
                    {"Jwt:Issuer", "TestIssuer"},
                    {"Jwt:Audience", "TestAudience"}
                })
                .Build();

            // Initialize History Service
            HistoryService = new HistoryService(DbContext);

            // Seed the database
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Seed API Users
            var testUsers = new List<ApiUser>
            {
                new ApiUser
                {
                    id = 1,
                    api_key = AdminApiKey,
                    app = "Admin Test App",
                    role = "Admin",
                    warehouse_id = null,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser
                {
                    id = 2,
                    api_key = WarehouseManagerApiKey,
                    app = "Warehouse Manager Test App",
                    role = "Warehouse_Manager",
                    warehouse_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser
                {
                    id = 3,
                    api_key = InventoryManagerApiKey,
                    app = "Inventory Manager Test App",
                    role = "Inventory_Manager",
                    warehouse_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser
                {
                    id = 4,
                    api_key = ViewerApiKey,
                    app = "Viewer Test App",
                    role = "Operative",
                    warehouse_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            DbContext.ApiUsers.AddRange(testUsers);

            // Seed Role Permissions
            var resources = new[] 
            { 
                "warehouses", "items", "clients", "orders", 
                "inventories", "shipments", "suppliers", "locations", 
                "transfers", "keys", "itemgroup", "itemlines", "itemtypes", "docks" 
            };

            var permissions = new List<RolePermission>();

            // Admin permissions - full access to everything
            foreach (var resource in resources)
            {
                permissions.Add(new RolePermission
                {
                    role = "Admin",
                    resource = resource,
                    can_view = true,
                    can_create = true,
                    can_update = true,
                    can_delete = true,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            // Warehouse Manager permissions
            foreach (var resource in new[] { "warehouses", "locations", "transfers" })
            {
                permissions.Add(new RolePermission
                {
                    role = "Warehouse_Manager",
                    resource = resource,
                    can_view = true,
                    can_create = false,
                    can_update = false,
                    can_delete = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            // Inventory Manager permissions
            foreach (var resource in new[] { "inventories", "items", "locations" })
            {
                permissions.Add(new RolePermission
                {
                    role = "Inventory_Manager",
                    resource = resource,
                    can_view = true,
                    can_create = resource == "inventories",
                    can_update = resource == "inventories",
                    can_delete = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            // Operative permissions
            foreach (var resource in new[] { "inventories", "items" })
            {
                permissions.Add(new RolePermission
                {
                    role = "Operative",
                    resource = resource,
                    can_view = true,
                    can_create = false,
                    can_update = false,
                    can_delete = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            DbContext.RolePermissions.AddRange(permissions);
            DbContext.SaveChanges();
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
            GC.SuppressFinalize(this);
        }

        // Static utility class for creating auth headers and setting up test users
        public static class TestAuthHandler
        {
            public static Dictionary<string, string> CreateAuthHeaders(string apiKey)
            {
                return new Dictionary<string, string>
                {
                    { "API_KEY", apiKey }
                };
            }

            // Renamed from SetupTestUser to be more flexible
            public static void SetupUserContext<TController>(TController controller, ApiUser user) 
                where TController : ControllerBase
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Items["User"] = user;
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                };
            }
        }

        // Convenience method to set up admin user context
        protected void SetupAdminUserContext<TController>(TController controller) 
            where TController : ControllerBase
        {
            var adminUser = DbContext.ApiUsers.FirstOrDefault(u => u.role == "Admin");
            if (adminUser != null)
            {
                TestAuthHandler.SetupUserContext(controller, adminUser);
            }
        }

        // Convenience method to set up a specific role user context
        protected void SetupUserContextByRole<TController>(TController controller, string role) 
            where TController : ControllerBase
        {
            var user = DbContext.ApiUsers.FirstOrDefault(u => u.role == role);
            if (user != null)
            {
                TestAuthHandler.SetupUserContext(controller, user);
            }
        }
    }
}
