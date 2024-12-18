using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class ItemGroupsControllerTests
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed initial data
            context.ItemGroups.AddRange(
                new ItemGroupModel
                {
                    id = 1,
                    name = "Electronics",
                    itemtype_id = 0,
                    description = "Electronic items",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemGroupModel
                {
                    id = 2,
                    name = "Furniture",
                    itemtype_id= 0,
                    description = "Furniture items",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAll_ReturnsAllItemGroups()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var result = await controller.GetAll() as OkObjectResult;
            var itemGroups = result?.Value as List<ItemGroupModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, itemGroups.Count);
        }

        [Fact]
        public async Task GetById_ReturnsItemGroups()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var result = await controller.GetById(1) as OkObjectResult;
            var itemGroups = result?.Value as ItemGroupModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(itemGroups);
            Xunit.Assert.Equal("Electronics", itemGroups.name);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.GetById(99));
            Xunit.Assert.Equal("ItemType not found!", exception.Message);
        }

        [Fact]
        public async Task Create_AddsNewItemGroups()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var newItemType = new ItemGroupModel
            {
                id = 3,
                name = "Clothing",
                description = "Clothing items",
                itemtype_id = 3,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            var result = await controller.Create(newItemType) as CreatedAtActionResult;
            var itemGroups = await dbContext.ItemType.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(3, itemGroups.Count);
            Xunit.Assert.Contains(itemGroups, it => it.name == "Clothing");
        }

        [Fact]
        public async Task Update_UpdatesExistingItemType()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var updatedItemGroups = new ItemGroupModel
            {
                name = "Electronics Updated",
                description = "Updated electronic items",
                updated_at = DateTime.UtcNow
            };

            var result = await controller.Update(1, updatedItemGroups) as OkObjectResult;
            var itemGroups = await dbContext.ItemType.FirstOrDefaultAsync(it => it.id == 1);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(itemGroups);
            Xunit.Assert.Equal("Electronics Updated", itemGroups.name);
        }

        [Fact]
        public async Task Update_ReturnsNotFoundForInvalidId()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var updatedItemGroups = new ItemGroupModel
            {
                name = "Non-Existent",
                description = "Invalid",
                updated_at = DateTime.UtcNow
            };

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.Update(99, updatedItemGroups));
            Xunit.Assert.Equal("ItemType not found!", exception.Message);
        }

        [Fact]
        public async Task Delete_RemovesItemGroups()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var result = await controller.Delete(1) as OkObjectResult;
            var itemGroups = await dbContext.ItemType.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(1, itemGroups.Count);
            Xunit.Assert.DoesNotContain(itemGroups, it => it.id == 1);
        }

        [Fact]
        public async Task Delete_ReturnsNotFoundForInvalidId()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.Delete(99));
            Xunit.Assert.Equal("ItemType not found!", exception.Message);
        }
    }
}
