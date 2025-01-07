using Xunit;
using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class ItemLineServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public ItemLineServiceTests()
        {
            // Set up In-Memory database options
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "ItemLineTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task GetAllItemLines_ReturnsListOfItemLines()
        {
            // Arrange: Create mock data for ItemLines
            var itemLineList = new List<ItemLineModel>
            {
                new ItemLineModel
                {
                    id = 1,
                    name = "Item Line A",
                    description = "Description for Item Line A",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemLineModel
                {
                    id = 2,
                    name = "Item Line B",
                    description = "Description for Item Line B",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            using (var context = CreateDbContext())
            {
                context.ItemLine.AddRange(itemLineList);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve all ItemLines
            IEnumerable<ItemLineModel> result;
            using (var context = CreateDbContext())
            {
                var service = new ItemLineService(context);
                result = await service.GetAllItemLines();
            }

            // Assert: Verify that the correct number of ItemLines is returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetItemLineById_ValidId_ReturnsItemLine()
        {
            // Arrange: Add an ItemLine to the database
            var itemLine = new ItemLineModel
            {
                id = 1,
                name = "Item Line A",
                description = "Description for Item Line A",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.ItemLine.Add(itemLine);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve the ItemLine by ID
            ItemLineModel result;
            using (var context = CreateDbContext())
            {
                var service = new ItemLineService(context);
                result = await service.GetItemLineById(1);
            }

            // Assert: Verify that the ItemLine was retrieved correctly
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Item Line A", result.name);
        }

        [Fact]
        public async Task CreateItemLine_ValidItemLine_CreatesItemLine()
        {
            // Arrange: Create a new ItemLine
            var newItemLine = new ItemLineModel
            {
                name = "Item Line C",
                description = "Description for Item Line C",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act: Add the new ItemLine to the database
            using (var context = CreateDbContext())
            {
                var service = new ItemLineService(context);
                await service.CreateItemLine(newItemLine);
            }

            // Assert: Verify that the ItemLine is added to the database
            using (var context = CreateDbContext())
            {
                var result = await context.ItemLine.FirstOrDefaultAsync(i => i.name == "Item Line C");
                Assert.NotNull(result);
                Assert.Equal("Item Line C", result.name);
            }
        }

        [Fact]
        public async Task UpdateItemLine_ValidItemLine_UpdatesItemLine()
        {
            // Arrange: Add an ItemLine to the database
            var itemLine = new ItemLineModel
            {
                id = 1,
                name = "Item Line A",
                description = "Description for Item Line A",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.ItemLine.Add(itemLine);
                await context.SaveChangesAsync();
            }

            // Act: Update the ItemLine
            var updatedItemLine = new ItemLineModel
            {
                name = "Updated Item Line A",
                description = "Updated description for Item Line A",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                var service = new ItemLineService(context);
                var result = await service.UpdateItemLine(1, updatedItemLine);
            }

            // Assert: Verify that the ItemLine was updated correctly
            using (var context = CreateDbContext())
            {
                var result = await context.ItemLine.FirstOrDefaultAsync(i => i.id == 1);
                Assert.NotNull(result);
                Assert.Equal("Updated Item Line A", result.name);
            }
        }

        [Fact]
        public async Task DeleteItemLine_ValidItemLine_DeletesItemLine()
        {
            // Arrange: Add an ItemLine to the database
            var itemLine = new ItemLineModel
            {
                id = 1,
                name = "Item Line A",
                description = "Description for Item Line A",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.ItemLine.Add(itemLine);
                await context.SaveChangesAsync();
            }

            // Act: Delete the ItemLine
            using (var context = CreateDbContext())
            {
                var service = new ItemLineService(context);
                await service.DeleteItemLine(1);
            }

            // Assert: Verify that the ItemLine was deleted
            using (var context = CreateDbContext())
            {
                var result = await context.ItemLine.FirstOrDefaultAsync(i => i.id == 1);
                Assert.Null(result); // The ItemLine should be deleted
            }
        }
    }
}
