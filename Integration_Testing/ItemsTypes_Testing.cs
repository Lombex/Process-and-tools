using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class ItemTypesControllerTests
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed initial data
            context.ItemType.AddRange(
                new ItemTypeModel
                {
                    id = 1,
                    name = "Electronics",
                    description = "Electronic items",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemTypeModel
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
        public async Task GetAll_ReturnsAllItemTypes()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemTypeService(dbContext);
            var controller = new ItemTypesController(service);

            var result = await controller.GetAll() as OkObjectResult;
            var itemTypes = result?.Value as List<ItemTypeModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, itemTypes.Count);
        }

        [Fact]
        public async Task GetById_ReturnsItemType()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemTypeService(dbContext);
            var controller = new ItemTypesController(service);

            var result = await controller.GetById(1) as OkObjectResult;
            var itemType = result?.Value as ItemTypeModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(itemType);
            Xunit.Assert.Equal("Electronics", itemType.name);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemTypeService(dbContext);
            var controller = new ItemTypesController(service);

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.GetById(99));
            Xunit.Assert.Equal("ItemType not found!", exception.Message);
        }

        [Fact]
        public async Task Create_AddsNewItemType()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemTypeService(dbContext);
            var controller = new ItemTypesController(service);

            var newItemType = new ItemTypeModel
            {
                id = 3,
                name = "Clothing",
                description = "Clothing items",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            var result = await controller.Create(newItemType) as CreatedAtActionResult;
            var itemTypes = await dbContext.ItemType.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(3, itemTypes.Count);
            Xunit.Assert.Contains(itemTypes, it => it.name == "Clothing");
        }

        [Fact]
        public async Task Update_UpdatesExistingItemType()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemTypeService(dbContext);
            var controller = new ItemTypesController(service);

            var updatedItemType = new ItemTypeModel
            {
                name = "Electronics Updated",
                description = "Updated electronic items",
                updated_at = DateTime.UtcNow
            };

            var result = await controller.Update(1, updatedItemType) as OkObjectResult;
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
            var service = new ItemTypeService(dbContext);
            var controller = new ItemTypesController(service);

            var updatedItemType = new ItemTypeModel
            {
                name = "Non-Existent",
                description = "Invalid",
                updated_at = DateTime.UtcNow
            };

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.Update(99, updatedItemType));
            Xunit.Assert.Equal("ItemType not found!", exception.Message);
        }

        [Fact]
        public async Task Delete_RemovesItemType()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemTypeService(dbContext);
            var controller = new ItemTypesController(service);

            var result = await controller.Delete(1) as OkObjectResult;
            var itemTypes = await dbContext.ItemType.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(1, itemTypes.Count);
            Xunit.Assert.DoesNotContain(itemTypes, it => it.id == 1);
        }

        [Fact]
        public async Task Delete_ReturnsNotFoundForInvalidId()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemTypeService(dbContext);
            var controller = new ItemTypesController(service);

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.Delete(99));
            Xunit.Assert.Equal("ItemType not found!", exception.Message);
        }
    }
}
