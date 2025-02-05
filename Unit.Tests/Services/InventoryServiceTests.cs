using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Unit.Tests.Services
{
    public class InventoriesServiceTests : IDisposable
    {
        private readonly SQLiteDatabase _db;
        private readonly IInventoriesService _service;

        public InventoriesServiceTests()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;
                
            _db = new SQLiteDatabase(options);
            _service = new InventoriesService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetAllInventories_ReturnsAllInventories()
        {
            // Arrange
            var inventories = new List<InventorieModel>
            {
                new InventorieModel 
                { 
                    id = 1, 
                    item_id = "P000001",
                    description = "Test Item 1",
                    total_on_hand = 10
                },
                new InventorieModel 
                { 
                    id = 2, 
                    item_id = "P000002",
                    description = "Test Item 2",
                    total_on_hand = 20
                }
            };
            await _db.Inventors.AddRangeAsync(inventories);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAllInventories();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(inventories[0].item_id, result[0].item_id);
            Assert.Equal(inventories[1].item_id, result[1].item_id);
        }

        [Fact]
        public async Task GetInventoryById_ReturnsCorrectInventory()
        {
            // Arrange
            var inventory = new InventorieModel 
            { 
                id = 1, 
                item_id = "P000001",
                description = "Test Item",
                total_on_hand = 10
            };
            await _db.Inventors.AddAsync(inventory);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetInventoryById(1);

            // Assert
            Assert.Equal(inventory.id, result.id);
            Assert.Equal(inventory.item_id, result.item_id);
        }

        [Fact]
        public async Task GetInventoryById_ThrowsException_WhenNotFound()
        {
            await Assert.ThrowsAsync<Exception>(() => _service.GetInventoryById(1));
        }

        [Fact]
        public async Task AddInventory_CreatesNewInventory()
        {
            // Arrange
            var inventory = new InventorieModel 
            { 
                item_id = "P000001",
                description = "Test Item",
                total_on_hand = 10,
                total_expected = 15,
                total_ordered = 5,
                total_allocated = 2,
                total_available = 8
            };

            // Act
            await _service.AddInventory(inventory);

            // Assert
            var result = await _db.Inventors.FirstOrDefaultAsync(x => x.item_id == "P000001");
            Assert.NotNull(result);
            Assert.Equal(inventory.description, result.description);
            Assert.Equal(inventory.total_on_hand, result.total_on_hand);
        }

        [Fact]
        public async Task UpdateInventory_UpdatesExistingInventory()
        {
            // Arrange
            var inventory = new InventorieModel 
            { 
                id = 1,
                item_id = "P000001",
                description = "Original Description",
                total_on_hand = 10
            };
            await _db.Inventors.AddAsync(inventory);
            await _db.SaveChangesAsync();

            var updatedInventory = new InventorieModel 
            { 
                id = 1,
                item_id = "P000001",
                description = "Updated Description",
                total_on_hand = 20
            };

            // Act
            var result = await _service.UpdateInventory(1, updatedInventory);

            // Assert
            Assert.True(result);
            var dbInventory = await _db.Inventors.FindAsync(1);
            Assert.Equal(updatedInventory.description, dbInventory.description);
            Assert.Equal(updatedInventory.total_on_hand, dbInventory.total_on_hand);
        }

        [Fact]
        public async Task UpdateInventory_ReturnsFalse_WhenInventoryNotFound()
        {
            // Act
            var result = await _service.UpdateInventory(1, new InventorieModel());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteInventory_DeletesAndArchivesInventory()
        {
            // Arrange
            var inventory = new InventorieModel 
            { 
                id = 1,
                item_id = "P000001",
                description = "Test Item",
                total_on_hand = 10
            };
            await _db.Inventors.AddAsync(inventory);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.DeleteInventory(1);

            // Assert
            Assert.True(result);
            var deletedInventory = await _db.Inventors.FindAsync(1);
            Assert.Null(deletedInventory);

            var archivedInventory = await _db.ArchivedInventories.FirstOrDefaultAsync();
            Assert.NotNull(archivedInventory);
            Assert.Equal(inventory.item_id, archivedInventory.item_id);
            Assert.Equal(inventory.description, archivedInventory.description);
        }

        [Fact]
        public async Task DeleteInventory_ReturnsFalse_WhenInventoryNotFound()
        {
            // Act
            var result = await _service.DeleteInventory(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetInventoriesByItemId_ReturnsCorrectInventories()
        {
            // Arrange
            var inventories = new List<InventorieModel>
            {
                new InventorieModel { id = 1, item_id = "P000001", description = "Test 1" },
                new InventorieModel { id = 2, item_id = "P000001", description = "Test 2" },
                new InventorieModel { id = 3, item_id = "P000002", description = "Test 3" }
            };
            await _db.Inventors.AddRangeAsync(inventories);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetInventoriesByItemId("P000001");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, i => Assert.Equal("P000001", i.item_id));
        }

        [Fact]
        public async Task GetInventoriesByLocation_ReturnsCorrectInventories()
        {
            // Arrange
            var inventories = new List<InventorieModel>
            {
                new InventorieModel { id = 1, item_id = "P000001", locations = new List<int> { 1, 2 } },
                new InventorieModel { id = 2, item_id = "P000002", locations = new List<int> { 2, 3 } },
                new InventorieModel { id = 3, item_id = "P000003", locations = new List<int> { 3, 4 } }
            };
            await _db.Inventors.AddRangeAsync(inventories);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetInventoriesByLocation(2);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, i => Assert.Contains(2, i.locations));
        }
    }
}
