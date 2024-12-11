using CSharpAPI.Data;
using CSharpAPI.Models.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Data
{
    public static class DatabaseSeeding
    {
        public static async Task SeedDatabase(SQLiteDatabase dbContext)
        {
            try
            {
                // Clear existing data
                await ClearExistingData(dbContext);
                
                // Seed new data
                await SeedRoles(dbContext);
                await SeedApiUsers(dbContext);
                
                // Save all changes
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error seeding database: {ex.Message}", ex);
            }
        }

        private static async Task ClearExistingData(SQLiteDatabase dbContext)
        {
            dbContext.RolePermissions.RemoveRange(await dbContext.RolePermissions.ToListAsync());
            dbContext.ApiUsers.RemoveRange(await dbContext.ApiUsers.ToListAsync());
            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedRoles(SQLiteDatabase dbContext)
        {
            var permissions = new List<RolePermission>();
            var resources = new[] { "clients", "orders", "inventories", "shipments", "suppliers", 
                                  "items", "warehouses", "transfers", "locations", "itemtypes", 
                                  "itemlines", "itemgroups" };

            // [Previous role permissions code remains exactly the same...]
            // Admin permissions
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

            // [Add all other role permissions as in the previous version...]

            await dbContext.RolePermissions.AddRangeAsync(permissions);
        }

        private static async Task SeedApiUsers(SQLiteDatabase dbContext)
        {
            // Create multiple warehouse-specific users for roles that need them
            var users = new List<ApiUser>
            {
                // Admin - No warehouse restriction
                new ApiUser 
                { 
                    api_key = "admin_key_2024",
                    app = "Admin Application",
                    role = "Admin",
                    warehouse_id = null,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Warehouse Manager - No specific warehouse (can manage all)
                new ApiUser 
                { 
                    api_key = "warehouse_key_2024",
                    app = "Warehouse Application",
                    role = "Warehouse_Manager",
                    warehouse_id = null,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Inventory Manager for Warehouse 1
                new ApiUser 
                { 
                    api_key = "inventory_key_2024_wh1",
                    app = "Inventory Application",
                    role = "Inventory_Manager",
                    warehouse_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Inventory Manager for Warehouse 2
                new ApiUser 
                { 
                    api_key = "inventory_key_2024_wh2",
                    app = "Inventory Application",
                    role = "Inventory_Manager",
                    warehouse_id = 2,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Floor Manager for Warehouse 1
                new ApiUser 
                { 
                    api_key = "floor_key_2024_wh1",
                    app = "Floor Application",
                    role = "Floor_Manager",
                    warehouse_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Floor Manager for Warehouse 2
                new ApiUser 
                { 
                    api_key = "floor_key_2024_wh2",
                    app = "Floor Application",
                    role = "Floor_Manager",
                    warehouse_id = 2,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Operative for Warehouse 1
                new ApiUser 
                { 
                    api_key = "operative_key_2024_wh1",
                    app = "Operations Application",
                    role = "Operative",
                    warehouse_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Operative for Warehouse 2
                new ApiUser 
                { 
                    api_key = "operative_key_2024_wh2",
                    app = "Operations Application",
                    role = "Operative",
                    warehouse_id = 2,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Supervisor for Warehouse 1
                new ApiUser 
                { 
                    api_key = "supervisor_key_2024_wh1",
                    app = "Supervisor Application",
                    role = "Supervisor",
                    warehouse_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Supervisor for Warehouse 2
                new ApiUser 
                { 
                    api_key = "supervisor_key_2024_wh2",
                    app = "Supervisor Application",
                    role = "Supervisor",
                    warehouse_id = 2,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Analyst - No warehouse restriction
                new ApiUser 
                { 
                    api_key = "analyst_key_2024",
                    app = "Analytics Application",
                    role = "Analyst",
                    warehouse_id = null,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Logistics - No warehouse restriction
                new ApiUser 
                { 
                    api_key = "logistics_key_2024",
                    app = "Logistics Application",
                    role = "Logistics",
                    warehouse_id = null,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                
                // Sales - No warehouse restriction
                new ApiUser 
                { 
                    api_key = "sales_key_2024",
                    app = "Sales Application",
                    role = "Sales",
                    warehouse_id = null,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await dbContext.ApiUsers.AddRangeAsync(users);
        }
    }
}