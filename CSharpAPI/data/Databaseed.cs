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
                await ClearExistingData(dbContext);
                await SeedRoles(dbContext);
                await SeedApiUsers(dbContext);
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
            var permissions = new List<RolePermission>
            {
                // Admin - Full access to everything
                new RolePermission { role = "Admin", resource = "clients", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Admin", resource = "orders", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Admin", resource = "inventories", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Admin", resource = "shipments", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Admin", resource = "suppliers", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Admin", resource = "items", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Admin", resource = "warehouses", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Admin", resource = "transfers", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Admin", resource = "locations", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Admin", resource = "itemtypes", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Admin", resource = "itemlines", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Admin", resource = "itemgroups", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },

                // Warehouse Manager
                new RolePermission { role = "Warehouse_Manager", resource = "clients", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Warehouse_Manager", resource = "orders", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Warehouse_Manager", resource = "inventories", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Warehouse_Manager", resource = "shipments", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Warehouse_Manager", resource = "suppliers", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Warehouse_Manager", resource = "items", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Warehouse_Manager", resource = "warehouses", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Warehouse_Manager", resource = "transfers", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Warehouse_Manager", resource = "locations", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Warehouse_Manager", resource = "itemtypes", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Warehouse_Manager", resource = "itemlines", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Warehouse_Manager", resource = "itemgroups", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },

                // Inventory Manager
                new RolePermission { role = "Inventory_Manager", resource = "clients", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Inventory_Manager", resource = "orders", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Inventory_Manager", resource = "inventories", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Inventory_Manager", resource = "shipments", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Inventory_Manager", resource = "suppliers", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Inventory_Manager", resource = "items", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Inventory_Manager", resource = "warehouses", can_view = false, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Inventory_Manager", resource = "transfers", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Inventory_Manager", resource = "locations", can_view = true, can_create = true, can_update = true, can_delete = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Inventory_Manager", resource = "itemtypes", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Inventory_Manager", resource = "itemlines", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Inventory_Manager", resource = "itemgroups", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },

                // Floor Manager
                new RolePermission { role = "Floor_Manager", resource = "clients", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Floor_Manager", resource = "orders", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Floor_Manager", resource = "inventories", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Floor_Manager", resource = "shipments", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Floor_Manager", resource = "suppliers", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Floor_Manager", resource = "items", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Floor_Manager", resource = "warehouses", can_view = false, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Floor_Manager", resource = "transfers", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Floor_Manager", resource = "locations", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },

                // Operative
                new RolePermission { role = "Operative", resource = "orders", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Operative", resource = "inventories", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Operative", resource = "shipments", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Operative", resource = "items", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Operative", resource = "transfers", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Operative", resource = "locations", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },

                // Supervisor
                new RolePermission { role = "Supervisor", resource = "orders", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Supervisor", resource = "inventories", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
               new RolePermission { role = "Supervisor", resource = "shipments", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Supervisor", resource = "suppliers", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Supervisor", resource = "items", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Supervisor", resource = "transfers", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Supervisor", resource = "locations", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },

                // Analyst
                new RolePermission { role = "Analyst", resource = "clients", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Analyst", resource = "orders", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Analyst", resource = "inventories", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Analyst", resource = "shipments", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Analyst", resource = "suppliers", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Analyst", resource = "items", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Analyst", resource = "warehouses", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Analyst", resource = "transfers", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Analyst", resource = "locations", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Analyst", resource = "itemtypes", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Analyst", resource = "itemlines", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Analyst", resource = "itemgroups", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },

                // Logistics
                new RolePermission { role = "Logistics", resource = "clients", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Logistics", resource = "orders", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Logistics", resource = "inventories", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Logistics", resource = "shipments", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Logistics", resource = "suppliers", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Logistics", resource = "items", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Logistics", resource = "warehouses", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Logistics", resource = "locations", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Logistics", resource = "itemtypes", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Logistics", resource = "itemlines", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Logistics", resource = "itemgroups", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },

                // Sales
                new RolePermission { role = "Sales", resource = "clients", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Sales", resource = "orders", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Sales", resource = "inventories", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Sales", resource = "shipments", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Sales", resource = "suppliers", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Sales", resource = "items", can_view = true, can_create = true, can_update = true, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Sales", resource = "warehouses", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Sales", resource = "locations", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Sales", resource = "itemtypes", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Sales", resource = "itemlines", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new RolePermission { role = "Sales", resource = "itemgroups", can_view = true, can_create = false, can_update = false, can_delete = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow }
            };

            await dbContext.RolePermissions.AddRangeAsync(permissions);
        }

        private static async Task SeedApiUsers(SQLiteDatabase dbContext)
        {
            var users = new List<ApiUser>
            {
                new ApiUser { api_key = "admin_key_2024", app = "Admin Application", role = "Admin", warehouse_id = null, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "warehouse_key_2024", app = "Warehouse Application", role = "Warehouse_Manager", warehouse_id = null, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "inventory_key_2024_wh1", app = "Inventory Application", role = "Inventory_Manager", warehouse_id = 1, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "inventory_key_2024_wh2", app = "Inventory Application", role = "Inventory_Manager", warehouse_id = 2, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "floor_key_2024_wh1", app = "Floor Application", role = "Floor_Manager", warehouse_id = 1, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "floor_key_2024_wh2", app = "Floor Application", role = "Floor_Manager", warehouse_id = 2, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "operative_key_2024_wh1", app = "Operations Application", role = "Operative", warehouse_id = 1, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "operative_key_2024_wh2", app = "Operations Application", role = "Operative", warehouse_id = 2, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "supervisor_key_2024_wh1", app = "Supervisor Application", role = "Supervisor", warehouse_id = 1, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "supervisor_key_2024_wh2", app = "Supervisor Application", role = "Supervisor", warehouse_id = 2, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "analyst_key_2024", app = "Analytics Application", role = "Analyst", warehouse_id = null, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "logistics_key_2024", app = "Logistics Application", role = "Logistics", warehouse_id = null, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ApiUser { api_key = "sales_key_2024", app = "Sales Application", role = "Sales", warehouse_id = null, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow }
            };

            await dbContext.ApiUsers.AddRangeAsync(users);
        }
    }
}
