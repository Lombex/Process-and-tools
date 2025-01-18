using CSharpAPI.Data;
using CSharpAPI.Models.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Tests.Infrastructure
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly SQLiteDatabase DbContext;
        protected readonly string AdminApiKey = "admin_test_key";
        protected readonly string ViewerApiKey = "viewer_test_key";

        protected IntegrationTestBase()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            DbContext = new SQLiteDatabase(options);
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Seed API Users
            DbContext.ApiUsers.AddRange(
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
                    api_key = ViewerApiKey,
                    app = "Viewer Test App",
                    role = "Viewer",
                    warehouse_id = null,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            );

            // Seed Role Permissions
            DbContext.RolePermissions.AddRange(
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

            DbContext.SaveChanges();
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
        }
    }

    public class TestAuthHandler
    {
        public static Dictionary<string, string> CreateAuthHeaders(string apiKey)
        {
            return new Dictionary<string, string>
            {
                { "API_KEY", apiKey }
            };
        }
    }
}