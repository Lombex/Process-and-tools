using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class ItemLinesControllerTests
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed initial data
            context.ItemLine.AddRange(
                new ItemLineModel
                {
                    id = 1,
                    name = "Electronics",
                    description = "Electronic items",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemLineModel
                {
                    id = 2,
                    name = "Furniture",
                    description = "Furniture items",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAll_ReturnsAllItemLines()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var result = await controller.GetAll() as OkObjectResult;
            var itemLines = result?.Value as List<ItemLineModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, itemLines.Count);
        }

        [Fact]
        public async Task GetById_ReturnsItemGroups()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var result = await controller.GetById(1) as OkObjectResult;
            var itemLines = result?.Value as ItemLineModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(itemLines);
            Xunit.Assert.Equal("Electronics", itemLines.name);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.GetById(99));
            Xunit.Assert.Equal("ItemType not found!", exception.Message);
        }

        [Fact]
        public async Task Create_AddsNewItemType()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var ItemLines = new ItemLineModel
            {
                id = 3,
                name = "Clothing",
                description = "Clothing items",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            var result = await controller.Create(ItemLines) as CreatedAtActionResult;
            var itemLines = await dbContext.ItemType.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(3, itemLines.Count);
            Xunit.Assert.Contains(itemLines, it => it.name == "Clothing");
        }

        [Fact]
        public async Task Update_UpdatesExistingItemType()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var updatedItemLines = new ItemLineModel
            {
                name = "Electronics Updated",
                description = "Updated electronic items",
                updated_at = DateTime.UtcNow
            };

            var result = await controller.Update(1, updatedItemLines) as OkObjectResult;
            var itemType = await dbContext.ItemType.FirstOrDefaultAsync(it => it.id == 1);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(itemType);
            Xunit.Assert.Equal("Electronics Updated", itemType.name);
        }

        [Fact]
        public async Task Update_ReturnsNotFoundForInvalidId()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var updatedItemLines = new ItemLineModel
            {
                name = "Non-Existent",
                description = "Invalid",
                updated_at = DateTime.UtcNow
            };

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.Update(99, updatedItemLines));
            Xunit.Assert.Equal("ItemType not found!", exception.Message);
        }

        [Fact]
        public async Task Delete_RemovesItemType()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var result = await controller.Delete(1) as OkObjectResult;
            var itemLines = await dbContext.ItemType.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(1, itemLines.Count);
            Xunit.Assert.DoesNotContain(itemLines, it => it.id == 1);
        }

        [Fact]
        public async Task Delete_ReturnsNotFoundForInvalidId()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.Delete(99));
            Xunit.Assert.Equal("ItemType not found!", exception.Message);
        }
    }
}
