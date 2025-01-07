using Xunit;
using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class InventoriesServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public InventoriesServiceTests()
        {
            // Set up In-Memory database options
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "InventoriesTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task GetAllInventories_ReturnsListOfInventories()
        {
            // Arrange: Create mock data for Inventories
            var inventoryList = new List<InventorieModel>
            {
                new InventorieModel
                {
                    id = 1,
                    item_id = "Item001",
                    description = "Inventory for Item 1",
                    item_reference = "Ref001",
                    locations = new List<int> { 1, 2 },
                    total_on_hand = 100,
                    total_expected = 120,
                    total_ordered = 50,
                    total_allocated = 30,
                    total_available = 40,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new InventorieModel
                {
                    id = 2,
                    item_id = "Item002",
                    description = "Inventory for Item 2",
                    item_reference = "Ref002",
                    locations = new List<int> { 2, 3 },
                    total_on_hand = 200,
                    total_expected = 220,
                    total_ordered = 80,
                    total_allocated = 60,
                    total_available = 60,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            using (var context = CreateDbContext())
            {
                context.Inventors.AddRange(inventoryList);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve all Inventories
            List<InventorieModel> result;
            using (var context = CreateDbContext())
            {
                var service = new InventoriesService(context);
                result = await service.GetAllInventories();
            }

            // Assert: Verify that the correct number of Inventories is returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetInventoryById_ValidId_ReturnsInventory()
        {
            // Arrange: Add an Inventory to the database
            var inventory = new InventorieModel
            {
                id = 1,
                item_id = "Item001",
                description = "Inventory for Item 1",
                item_reference = "Ref001",
                locations = new List<int> { 1, 2 },
                total_on_hand = 100,
                total_expected = 120,
                total_ordered = 50,
                total_allocated = 30,
                total_available = 40,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.Inventors.Add(inventory);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve the Inventory by ID
            InventorieModel result;
            using (var context = CreateDbContext())
            {
                var service = new InventoriesService(context);
                result = await service.GetInventoryById(1);
            }

            // Assert: Verify that the Inventory was retrieved correctly
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Item001", result.item_id);
        }

        [Fact]
        public async Task AddInventory_ValidInventory_CreatesInventory()
        {
            // Arrange: Create a new Inventory
            var newInventory = new InventorieModel
            {
                item_id = "Item003",
                description = "Inventory for Item 3",
                item_reference = "Ref003",
                locations = new List<int> { 3, 4 },
                total_on_hand = 300,
                total_expected = 350,
                total_ordered = 150,
                total_allocated = 100,
                total_available = 100,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act: Add the new Inventory to the database
            using (var context = CreateDbContext())
            {
                var service = new InventoriesService(context);
                await service.AddInventory(newInventory);
            }

            // Assert: Verify that the Inventory is added to the database
            using (var context = CreateDbContext())
            {
                var result = await context.Inventors.FirstOrDefaultAsync(i => i.item_id == "Item003");
                Assert.NotNull(result);
                Assert.Equal("Item003", result.item_id);
            }
        }

        [Fact]
        public async Task UpdateInventory_ValidInventory_UpdatesInventory()
        {
            // Arrange: Add an Inventory to the database
            var inventory = new InventorieModel
            {
                id = 1,
                item_id = "Item001",
                description = "Inventory for Item 1",
                item_reference = "Ref001",
                locations = new List<int> { 1, 2 },
                total_on_hand = 100,
                total_expected = 120,
                total_ordered = 50,
                total_allocated = 30,
                total_available = 40,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.Inventors.Add(inventory);
                await context.SaveChangesAsync();
            }

            // Act: Update the Inventory
            var updatedInventory = new InventorieModel
            {
                id = 1,
                item_id = "Item001",
                description = "Updated Inventory for Item 1",
                item_reference = "Ref001Updated",
                locations = new List<int> { 1, 3 },
                total_on_hand = 150,
                total_expected = 160,
                total_ordered = 60,
                total_allocated = 40,
                total_available = 50,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                var service = new InventoriesService(context);
                var result = await service.UpdateInventory(1, updatedInventory);
            }

            // Assert: Verify that the Inventory was updated correctly
            using (var context = CreateDbContext())
            {
                var result = await context.Inventors.FirstOrDefaultAsync(i => i.id == 1);
                Assert.NotNull(result);
                Assert.Equal("Updated Inventory for Item 1", result.description);
            }
        }

        [Fact]
        public async Task DeleteInventory_ValidInventory_DeletesInventory()
        {
            // Arrange: Add an Inventory to the database
            var inventory = new InventorieModel
            {
                id = 1,
                item_id = "Item001",
                description = "Inventory for Item 1",
                item_reference = "Ref001",
                locations = new List<int> { 1, 2 },
                total_on_hand = 100,
                total_expected = 120,
                total_ordered = 50,
                total_allocated = 30,
                total_available = 40,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.Inventors.Add(inventory);
                await context.SaveChangesAsync();
            }

            // Act: Delete the Inventory
            using (var context = CreateDbContext())
            {
                var service = new InventoriesService(context);
                await service.DeleteInventory(1);
            }

            // Assert: Verify that the Inventory was deleted
            using (var context = CreateDbContext())
            {
                var result = await context.Inventors.FirstOrDefaultAsync(i => i.id == 1);
                Assert.Null(result); // The Inventory should be deleted
            }
        }

        [Fact]
        public async Task GetInventoriesByItemId_ValidItemId_ReturnsInventories()
        {
            // Arrange: Add inventories to the database
            var inventoryList = new List<InventorieModel>
            {
                new InventorieModel
                {
                    id = 1,
                    item_id = "Item001",
                    description = "Inventory for Item 1",
                    item_reference = "Ref001",
                    locations = new List<int> { 1, 2 },
                    total_on_hand = 100,
                    total_expected = 120,
                    total_ordered = 50,
                    total_allocated = 30,
                    total_available = 40,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new InventorieModel
                {
                    id = 2,
                    item_id = "Item001",
                    description = "Another Inventory for Item 1",
                    item_reference = "Ref002",
                    locations = new List<int> { 2, 3 },
                    total_on_hand = 200,
                    total_expected = 220,
                    total_ordered = 80,
                    total_allocated = 60,
                    total_available = 60,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            using (var context = CreateDbContext())
            {
                context.Inventors.AddRange(inventoryList);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve inventories by ItemId
            List<InventorieModel> result;
            using (var context = CreateDbContext())
            {
                var service = new InventoriesService(context);
                result = await service.GetInventoriesByItemId("Item001");
            }

            // Assert: Verify that the correct inventories are returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetInventoriesByLocation_ValidLocationId_ReturnsInventories()
        {
            // Arrange: Add inventories to the database
            var inventoryList = new List<InventorieModel>
            {
                new InventorieModel
                {
                    id = 1,
                    item_id = "Item001",
                    description = "Inventory for Item 1",
                    item_reference = "Ref001",
                    locations = new List<int> { 1, 2 },
                    total_on_hand = 100,
                    total_expected = 120,
                    total_ordered = 50,
                    total_allocated = 30,
                    total_available = 40,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new InventorieModel
                {
                    id = 2,
                    item_id = "Item002",
                    description = "Inventory for Item 2",
                    item_reference = "Ref002",
                    locations = new List<int> { 2, 3 },
                    total_on_hand = 200,
                    total_expected = 220,
                    total_ordered = 80,
                    total_allocated = 60,
                    total_available = 60,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            using (var context = CreateDbContext())
            {
                context.Inventors.AddRange(inventoryList);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve inventories by LocationId
            List<InventorieModel> result;
            using (var context = CreateDbContext())
            {
                var service = new InventoriesService(context);
                result = await service.GetInventoriesByLocation(2);
            }

            // Assert: Verify that the correct inventories are returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}
