using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class InventoriesControllerTests
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed initial data
            context.Inventors.AddRange(
                new InventorieModel
                {
                    id = 1,
                    item_id = "ITEM001",
                    description = "First Inventory Item",
                    item_reference = "REF001",
                    locations = new List<int> { 1, 2 },
                    total_on_hand = 100,
                    total_expected = 150,
                    total_ordered = 50,
                    total_allocated = 20,
                    total_available = 80,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new InventorieModel
                {
                    id = 2,
                    item_id = "ITEM002",
                    description = "Second Inventory Item",
                    item_reference = "REF002",
                    locations = new List<int> { 3, 4 },
                    total_on_hand = 200,
                    total_expected = 250,
                    total_ordered = 75,
                    total_allocated = 25,
                    total_available = 175,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAllInventories_ReturnsAllInventories()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            // Act
            var result = await controller.GetAllInventories() as OkObjectResult;
            var inventories = result?.Value as List<InventorieModel>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(2, inventories.Count);
        }

        [Fact]
        public async Task GetInventoryById_ReturnsInventory()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            // Act
            var result = await controller.GetInventoryById(1) as OkObjectResult;
            var inventory = result?.Value as InventorieModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("ITEM001", inventory.item_id);
        }

        [Fact]
        public async Task GetInventoryById_NotFound_ReturnsNotFound()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            // Act
            var result = await controller.GetInventoryById(99);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateInventory_AddsNewInventory()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            var newInventory = new InventorieModel
            {
                item_id = "ITEM003",
                description = "Third Inventory Item",
                item_reference = "REF003",
                locations = new List<int> { 5, 6 },
                total_on_hand = 300,
                total_expected = 350,
                total_ordered = 100,
                total_allocated = 30,
                total_available = 270
            };

            // Act
            var result = await controller.CreateInventory(newInventory) as CreatedAtActionResult;
            var inventories = await dbContext.Inventors.ToListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(3, inventories.Count);
            Assert.Contains(inventories, i => i.item_id == "ITEM003");
        }

        [Fact]
        public async Task CreateInventory_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            // Act
            var result = await controller.CreateInventory(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateInventory_UpdatesExistingInventory()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            var updatedInventory = new InventorieModel
            {
                item_id = "ITEM001-UPD",
                description = "Updated First Inventory",
                total_on_hand = 150,
                total_available = 130
            };

            // Act
            var result = await controller.UpdateInventory(1, updatedInventory);
            var inventory = await dbContext.Inventors.FindAsync(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal("ITEM001-UPD", inventory.item_id);
            Assert.Equal(150, inventory.total_on_hand);
        }

        [Fact]
        public async Task UpdateInventory_NonExistent_ReturnsNotFound()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            var updatedInventory = new InventorieModel
            {
                item_id = "NON-EXISTENT",
                description = "Non-existent Inventory"
            };

            // Act
            var result = await controller.UpdateInventory(99, updatedInventory);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteInventory_RemovesInventory()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            // Act
            var result = await controller.DeleteInventory(1);
            var inventories = await dbContext.Inventors.ToListAsync();

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Single(inventories);
            Assert.DoesNotContain(inventories, i => i.id == 1);
        }

        [Fact]
        public async Task DeleteInventory_NonExistent_ReturnsNotFound()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            // Act
            var result = await controller.DeleteInventory(99);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetInventoriesByItemId_ReturnsInventories()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            // Act
            var result = await controller.GetInventoriesByItemId("ITEM001") as OkObjectResult;
            var inventories = result?.Value as List<InventorieModel>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Single(inventories);
            Assert.Equal("ITEM001", inventories[0].item_id);
        }

        [Fact]
        public async Task GetInventoriesByLocation_ReturnsInventories()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            // Act
            var result = await controller.GetInventoriesByLocation(1) as OkObjectResult;
            var inventories = result?.Value as List<InventorieModel>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Single(inventories);
            Assert.Contains(1, inventories[0].locations);
        }
    }
}