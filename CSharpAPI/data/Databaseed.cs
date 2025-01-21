using CSharpAPI.Data;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Data
{
    public static class DatabaseSeeding
    {
        public static async Task SeedDatabase(SQLiteDatabase dbContext, IAuthService authService)
        {
            try
            {
                // Remove existing permissions to start fresh
                dbContext.RolePermissions.RemoveRange(await dbContext.RolePermissions.ToListAsync());
                await dbContext.SaveChangesAsync();

                // Only seed default API users if none exist
                if (!await dbContext.ApiUsers.AnyAsync())
                {
                    await SeedApiUsers(dbContext);
                }
                
                // Seed default admin role permissions
                await authService.EnsureRolePermissions("Admin");

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error seeding database: {ex.Message}", ex);
            }
        }

        private static async Task SeedApiUsers(SQLiteDatabase dbContext)
        {
            var users = new List<ApiUser>
            {
                new ApiUser { 
                    api_key = "admin_key_2024", 
                    app = "Admin Application", 
                    role = "Admin", 
                    warehouse_id = null, 
                    created_at = DateTime.UtcNow, 
                    updated_at = DateTime.UtcNow 
                }
            };

            await dbContext.ApiUsers.AddRangeAsync(users);
        }
    }
}
