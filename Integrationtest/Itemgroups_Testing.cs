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
                    name = "Electronics Group",
                    description = "Electronic items group",
                    itemtype_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemGroupModel
                {
                    id = 2,
                    name = "Furniture Group",
                    description = "Furniture items group",
                    itemtype_id = 2,
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
        public async Task GetById_ReturnsItemGroup()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var result = await controller.GetById(1) as OkObjectResult;
            var itemGroup = result?.Value as ItemGroupModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(itemGroup);
            Xunit.Assert.Equal("Electronics Group", itemGroup.name);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var result = await controller.GetById(99) as NotFoundObjectResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(404, result.StatusCode);
            Xunit.Assert.Equal("ItemGroup 99 not found", result.Value);
        }

        [Fact]
        public async Task Create_AddsNewItemGroup()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var newItemGroup = new ItemGroupModel
            {
                id = 3,
                name = "Tools Group",
                description = "Tools and equipment",
                itemtype_id = 3,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            var result = await controller.Create(newItemGroup) as CreatedAtActionResult;
            var itemGroups = await dbContext.ItemGroups.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(3, itemGroups.Count);
            Xunit.Assert.Contains(itemGroups, g => g.name == "Tools Group");
        }

        [Fact]
        public async Task Update_UpdatesExistingItemGroup()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var updatedItemGroup = new ItemGroupModel
            {
                name = "Electronics Group Updated",
                description = "Updated electronic items group",
                itemtype_id = 1,
                updated_at = DateTime.UtcNow
            };

            var result = await controller.Update(1, updatedItemGroup) as OkObjectResult;
            var itemGroup = await dbContext.ItemGroups.FirstOrDefaultAsync(g => g.id == 1);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(itemGroup);
            Xunit.Assert.Equal("Electronics Group Updated", itemGroup.name);
        }

        [Fact]
        public async Task Update_ReturnsNotFoundForInvalidId()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var updatedItemGroup = new ItemGroupModel
            {
                name = "Non-existent Group",
                description = "Invalid group",
                itemtype_id = 1,
                updated_at = DateTime.UtcNow
            };

            var result = await controller.Update(99, updatedItemGroup) as NotFoundObjectResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(404, result.StatusCode);
            Xunit.Assert.Equal("ItemGroup 99 not found", result.Value);
        }

        [Fact]
        public async Task Delete_RemovesItemGroup()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var result = await controller.Delete(1) as OkObjectResult;
            var itemGroups = await dbContext.ItemGroups.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(itemGroups);
            Xunit.Assert.DoesNotContain(itemGroups, g => g.id == 1);
        }

        [Fact]
        public async Task Delete_ReturnsNotFoundForInvalidId()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemGroupService(dbContext);
            var controller = new ItemGroupsController(service);

            var result = await controller.Delete(99) as NotFoundObjectResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(404, result.StatusCode);
            Xunit.Assert.Equal("ItemGroup 99 not found", result.Value);
        }
    }
}