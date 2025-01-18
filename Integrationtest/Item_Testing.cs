using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class ItemsControllerTests
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            context.itemModels.AddRange(
                new ItemModel
                {
                    uid = "P000001",
                    code = "ITEM001",
                    description = "First item",
                    short_description = "Item 1",
                    upc_code = "123456789",
                    model_number = "M001",
                    commodity_code = "C001",
                    item_line = 1,
                    item_group = 1,
                    item_type = 1,
                    supplier_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemModel
                {
                    uid = "P000002",
                    code = "ITEM002",
                    description = "Second item",
                    short_description = "Item 2",
                    upc_code = "987654321",
                    model_number = "M002",
                    commodity_code = "C002",
                    item_line = 2,
                    item_group = 2,
                    item_type = 2,
                    supplier_id = 2,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAllItems_ReturnsAllItems()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var result = await controller.GetAllItems() as OkObjectResult;
            var items = result?.Value as List<ItemModel>;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task GetItemById_ReturnsItem()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var result = await controller.GetItemById("P000001") as OkObjectResult;
            var item = result?.Value as ItemModel;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("ITEM001", item.code);
        }

        [Fact]
        public async Task GetItemsByLineId_ReturnsItems()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var result = await controller.GetItemsByLineId(1) as OkObjectResult;
            var items = result?.Value as IEnumerable<ItemModel>;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Single(items);
        }

        [Fact]
        public async Task CreateItem_AddsNewItem()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var newItem = new ItemModel
            {
                code = "ITEM003",
                description = "Third item",
                item_line = 1,
                item_group = 1,
                item_type = 1
            };

            var result = await controller.CreateItem(newItem) as CreatedAtActionResult;
            var items = await dbContext.itemModels.ToListAsync();

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(3, items.Count);
        }

        [Fact]
        public async Task UpdateItem_UpdatesExistingItem()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var updatedItem = new ItemModel
            {
                code = "ITEM001-UPD",
                description = "Updated first item"
            };

            var result = await controller.UpdateItem("P000001", updatedItem) as OkObjectResult;
            var item = await dbContext.itemModels.FindAsync("P000001");

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("ITEM001-UPD", item.code);
        }

        [Fact]
        public async Task DeleteItem_RemovesItem()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var result = await controller.DeleteItem("P000001") as OkObjectResult;
            var items = await dbContext.itemModels.ToListAsync();

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Single(items);
        }
    }
}