using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CSharpAPI.Tests
{
    public class ItemTypeServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public ItemTypeServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "ItemTypeTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task Add_ItemType_AddsSuccessfully()
        {
            var newItemType = new ItemTypeModel
            {
                name = "Type A",
                description = "Description for Type A",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act: Add a new ItemType to the database
            using (var context = CreateDbContext())
            {
                var service = new ItemTypeService(context);
                await service.Add(newItemType);
            }

            // Assert: Verify the ItemType has been added to the database
            using (var context = CreateDbContext())
            {
                var result = await context.ItemType.FirstOrDefaultAsync(i => i.name == "Type A");
                Assert.NotNull(result);
                Assert.Equal("Type A", result.name);
            }
        }

        [Fact]
        public async Task GetAll_ReturnsAllItemTypes()
        {
            // Arrange: Add mock ItemTypes to the database
            var itemTypes = new List<ItemTypeModel>
            {
                new ItemTypeModel { name = "Type A", description = "Description A", created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow },
                new ItemTypeModel { name = "Type B", description = "Description B", created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow }
            };

            using (var context = CreateDbContext())
            {
                context.ItemType.AddRange(itemTypes);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve all ItemTypes
            List<ItemTypeModel> result;
            using (var context = CreateDbContext())
            {
                var service = new ItemTypeService(context);
                result = await service.GetAll();
            }

            // Assert: Verify the correct number of ItemTypes is returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsItemType()
        {
            // Arrange: Add a mock ItemType
            var itemType = new ItemTypeModel { name = "Type A", description = "Description A", created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow };

            using (var context = CreateDbContext())
            {
                context.ItemType.Add(itemType);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve ItemType by ID
            ItemTypeModel result;
            using (var context = CreateDbContext())
            {
                var service = new ItemTypeService(context);
                result = await service.GetById(1);  // Assuming ID 1
            }

            // Assert: Verify that the correct ItemType is returned
            Assert.NotNull(result);
            Assert.Equal("Type A", result.name);
        }

        [Fact]
        public async Task Update_ValidId_UpdatesItemType()
        {
            // Arrange: Add an ItemType
            var itemType = new ItemTypeModel { name = "Type A", description = "Description A", created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow };
            using (var context = CreateDbContext())
            {
                context.ItemType.Add(itemType);
                await context.SaveChangesAsync();
            }

            // Act: Update the ItemType
            var updatedItemType = new ItemTypeModel { name = "Updated Type A", description = "Updated Description A", created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow };
            using (var context = CreateDbContext())
            {
                var service = new ItemTypeService(context);
                await service.Update(1, updatedItemType);
            }

            // Assert: Verify the ItemType has been updated
            using (var context = CreateDbContext())
            {
                var result = await context.ItemType.FirstOrDefaultAsync(i => i.id == 1);
                Assert.NotNull(result);
                Assert.Equal("Updated Type A", result.name);
            }
        }

        [Fact]
        public async Task Delete_ValidId_DeletesItemType()
        {
            // Arrange: Add an ItemType
            var itemType = new ItemTypeModel { name = "Type A", description = "Description A", created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow };
            using (var context = CreateDbContext())
            {
                context.ItemType.Add(itemType);
                await context.SaveChangesAsync();
            }

            // Act: Delete the ItemType by ID
            using (var context = CreateDbContext())
            {
                var service = new ItemTypeService(context);
                await service.Delete(1);
            }

            // Assert: Verify the ItemType has been deleted
            using (var context = CreateDbContext())
            {
                var result = await context.ItemType.FirstOrDefaultAsync(i => i.id == 1);
                Assert.Null(result); // The ItemType should be deleted
            }
        }
    }
}
