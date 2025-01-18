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

            context.ItemLine.AddRange(
                new ItemLineModel
                {
                    id = 1,
                    name = "Line One",
                    description = "First product line",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemLineModel
                {
                    id = 2,
                    name = "Line Two",
                    description = "Second product line",
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
            var itemLines = result?.Value as IEnumerable<ItemLineModel>;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(2, itemLines.Count());
        }

        [Fact]
        public async Task GetById_ReturnsItemLine()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var result = await controller.GetById(1) as OkObjectResult;
            var itemLine = result?.Value as ItemLineModel;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Line One", itemLine.name);
        }

        [Fact]
        public async Task Create_AddsNewItemLine()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var newItemLine = new ItemLineModel
            {
                name = "Line Three",
                description = "Third product line"
            };

            var result = await controller.Create(newItemLine) as CreatedAtActionResult;
            var itemLines = await dbContext.ItemLine.ToListAsync();

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(3, itemLines.Count);
        }

        [Fact]
        public async Task Update_UpdatesExistingItemLine()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var updatedItemLine = new ItemLineModel
            {
                name = "Updated Line One",
                description = "Updated description"
            };

            var result = await controller.Update(1, updatedItemLine) as OkObjectResult;
            var itemLine = await dbContext.ItemLine.FindAsync(1);

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Updated Line One", itemLine.name);
        }

        [Fact]
        public async Task Delete_RemovesItemLine()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemLineService(dbContext);
            var controller = new ItemLinesController(service);

            var result = await controller.Delete(1) as OkObjectResult;
            var itemLines = await dbContext.ItemLine.ToListAsync();

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Single(itemLines);
        }
    }
}