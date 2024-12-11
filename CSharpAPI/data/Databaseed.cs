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
            // Remove all existing role permissions
            dbContext.RolePermissions.RemoveRange(await dbContext.RolePermissions.ToListAsync());
            
            // Remove all existing API users
            dbContext.ApiUsers.RemoveRange(await dbContext.ApiUsers.ToListAsync());
            
            // Save the removal of existing data
            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedRoles(SQLiteDatabase dbContext)
        {
            var permissions = new List<RolePermission>();
            var resources = new[] { "clients", "orders", "inventories", "shipments", "suppliers", 
                                  "items", "warehouses", "transfers", "locations", "itemtypes", 
                                  "itemlines", "itemgroups" };

            // Admin - Full access to everything
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

            // Warehouse Manager
            foreach (var resource in resources)
            {
                var canDelete = resource switch
                {
                    "orders" or "inventories" or "shipments" or "transfers" or "locations" => true,
                    _ => false
                };

                var canCreate = resource != "warehouses" && resource != "clients";
                var canUpdate = canCreate;

                permissions.Add(new RolePermission
                {
                    role = "Warehouse_Manager",
                    resource = resource,
                    can_view = true,
                    can_create = canCreate,
                    can_update = canUpdate,
                    can_delete = canDelete,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            // Inventory Manager
            foreach (var resource in resources)
            {
                var canModify = resource switch
                {
                    "inventories" or "transfers" or "locations" or 
                    "itemtypes" or "itemlines" or "itemgroups" => true,
                    _ => false
                };

                permissions.Add(new RolePermission
                {
                    role = "Inventory_Manager",
                    resource = resource,
                    can_view = true,
                    can_create = canModify,
                    can_update = canModify,
                    can_delete = canModify,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            // Floor Manager
            foreach (var resource in resources)
            {
                var canModify = resource switch
                {
                    "transfers" or "locations" => true,
                    _ => false
                };

                permissions.Add(new RolePermission
                {
                    role = "Floor_Manager",
                    resource = resource,
                    can_view = true,
                    can_create = canModify,
                    can_update = canModify,
                    can_delete = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            // Operative
            foreach (var resource in resources)
            {
                var canView = resource switch
                {
                    "orders" or "inventories" or "shipments" or 
                    "items" or "transfers" or "locations" => true,
                    _ => false
                };

                var canModify = resource == "transfers";

                permissions.Add(new RolePermission
                {
                    role = "Operative",
                    resource = resource,
                    can_view = canView,
                    can_create = canModify,
                    can_update = canModify,
                    can_delete = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            // Supervisor
            foreach (var resource in resources)
            {
                var canView = resource switch
                {
                    "orders" or "inventories" or "shipments" or 
                    "items" or "transfers" or "locations" or "suppliers" => true,
                    _ => false
                };

                var canModify = resource == "transfers";

                permissions.Add(new RolePermission
                {
                    role = "Supervisor",
                    resource = resource,
                    can_view = canView,
                    can_create = canModify,
                    can_update = canModify,
                    can_delete = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            // Analyst
            foreach (var resource in resources)
            {
                permissions.Add(new RolePermission
                {
                    role = "Analyst",
                    resource = resource,
                    can_view = true,
                    can_create = false,
                    can_update = false,
                    can_delete = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            // Logistics
            foreach (var resource in resources)
            {
                var canModify = resource switch
                {
                    "clients" or "orders" or "inventories" or 
                    "shipments" or "suppliers" or "items" => true,
                    _ => false
                };

                permissions.Add(new RolePermission
                {
                    role = "Logistics",
                    resource = resource,
                    can_view = true,
                    can_create = canModify,
                    can_update = canModify,
                    can_delete = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            // Sales
            foreach (var resource in resources)
            {
                var canModify = resource switch
                {
                    "clients" or "orders" or "suppliers" or "items" => true,
                    _ => false
                };

                permissions.Add(new RolePermission
                {
                    role = "Sales",
                    resource = resource,
                    can_view = true,
                    can_create = canModify,
                    can_update = canModify,
                    can_delete = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            await dbContext.RolePermissions.AddRangeAsync(permissions);
        }

        private static async Task SeedApiUsers(SQLiteDatabase dbContext)
        {
            var users = new List<ApiUser>
            {
                new ApiUser 
                { 
                    api_key = "admin_key_2024",
                    app = "Admin Application",
                    role = "Admin",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser 
                { 
                    api_key = "warehouse_key_2024",
                    app = "Warehouse Application",
                    role = "Warehouse_Manager",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser 
                { 
                    api_key = "inventory_key_2024",
                    app = "Inventory Application",
                    role = "Inventory_Manager",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser 
                { 
                    api_key = "floor_key_2024",
                    app = "Floor Application",
                    role = "Floor_Manager",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser 
                { 
                    api_key = "operative_key_2024",
                    app = "Operations Application",
                    role = "Operative",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser 
                { 
                    api_key = "supervisor_key_2024",
                    app = "Supervisor Application",
                    role = "Supervisor",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser 
                { 
                    api_key = "analyst_key_2024",
                    app = "Analytics Application",
                    role = "Analyst",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser 
                { 
                    api_key = "logistics_key_2024",
                    app = "Logistics Application",
                    role = "Logistics",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser 
                { 
                    api_key = "sales_key_2024",
                    app = "Sales Application",
                    role = "Sales",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await dbContext.ApiUsers.AddRangeAsync(users);
        }
    }
}