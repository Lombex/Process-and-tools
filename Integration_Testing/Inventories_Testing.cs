using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class Inventories_Testing
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
                    item_id = "ITEM1",
                    description = "Item 1 Description",
                    item_reference = "REF1",
                    locations = new List<int> { 1, 2 },
                    total_on_hand = 100,
                    total_expected = 50,
                    total_ordered = 20,
                    total_allocated = 30,
                    total_available = 40,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new InventorieModel
                {
                    id = 2,
                    item_id = "ITEM2",
                    description = "Item 2 Description",
                    item_reference = "REF2",
                    locations = new List<int> { 3 },
                    total_on_hand = 200,
                    total_expected = 80,
                    total_ordered = 60,
                    total_allocated = 50,
                    total_available = 90,
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
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            var result = await controller.GetAllInventories() as OkObjectResult;
            var inventories = result?.Value as List<InventorieModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, inventories.Count);
        }

        [Fact]
        public async Task GetInventoryById_ReturnsInventory()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            var result = await controller.GetInventoryById(1) as OkObjectResult;
            var inventory = result?.Value as InventorieModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(inventory);
            Xunit.Assert.Equal("ITEM1", inventory.item_id);
        }

        [Fact]
        public async Task GetInventoryById_ReturnsNotFoundForInvalidId()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            var result = await controller.GetInventoryById(99) as NotFoundObjectResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(404, result.StatusCode);
            Xunit.Assert.Equal("Inventory with id 99 not found.", result.Value);
        }

        [Fact]
        public async Task CreateInventory_AddsNewInventory()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            var newInventory = new InventorieModel
            {
                id = 3,
                item_id = "ITEM3",
                description = "Item 3 Description",
                item_reference = "REF3",
                locations = new List<int> { 4 },
                total_on_hand = 150,
                total_expected = 70,
                total_ordered = 30,
                total_allocated = 40,
                total_available = 110,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var result = await controller.CreateInventory(newInventory) as CreatedAtActionResult;
            var inventories = await dbContext.Inventors.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(3, inventories.Count);
            Xunit.Assert.Contains(inventories, i => i.item_id == "ITEM3");
        }

        [Fact]
        public async Task UpdateInventory_UpdatesExistingInventory()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            var updatedInventory = new InventorieModel
            {
                item_id = "ITEM1-UPDATED",
                description = "Updated Item 1 Description",
                item_reference = "REF1-UPDATED",
                locations = new List<int> { 1 },
                total_on_hand = 110,
                total_expected = 60,
                total_ordered = 25,
                total_allocated = 35,
                total_available = 50,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var result = await controller.UpdateInventory(1, updatedInventory) as NoContentResult;
            var inventory = await dbContext.Inventors.FirstOrDefaultAsync(i => i.id == 1);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(204, result.StatusCode);
            Xunit.Assert.NotNull(inventory);
            Xunit.Assert.Equal("ITEM1-UPDATED", inventory.item_id);
        }

        [Fact]
        public async Task DeleteInventory_RemovesInventory()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            var result = await controller.DeleteInventory(1) as NoContentResult;
            var inventories = await dbContext.Inventors.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(204, result.StatusCode);
            Xunit.Assert.Single(inventories);
            Xunit.Assert.DoesNotContain(inventories, i => i.id == 1);
        }

        [Fact]
        public async Task GetInventoriesByItemId_ReturnsCorrectInventories()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            var result = await controller.GetInventoriesByItemId("ITEM1") as OkObjectResult;
            var inventories = result?.Value as List<InventorieModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(inventories);
            Xunit.Assert.Equal("ITEM1", inventories[0].item_id);
        }

        [Fact]
        public async Task GetInventoriesByLocation_ReturnsCorrectInventories()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new InventoriesService(dbContext);
            var controller = new InventoriesController(service);

            var result = await controller.GetInventoriesByLocation(1) as OkObjectResult;
            var inventories = result?.Value as List<InventorieModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(inventories);
            Xunit.Assert.Contains(1, inventories[0].locations);
        }
    }
}