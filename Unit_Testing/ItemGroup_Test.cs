using Xunit;
using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class ItemGroupServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public ItemGroupServiceTests()
        {
            // Set up In-Memory database options
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "ItemGroupTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task GetAll_ReturnsListOfItemGroups()
        {
            // Arrange: Create mock data for ItemGroups
            var itemGroupList = new List<ItemGroupModel>
            {
                new ItemGroupModel
                {
                    id = 1,
                    name = "Group A",
                    description = "Description for Group A",
                    itemtype_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemGroupModel
                {
                    id = 2,
                    name = "Group B",
                    description = "Description for Group B",
                    itemtype_id = 2,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            using (var context = CreateDbContext())
            {
                context.ItemGroups.AddRange(itemGroupList);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve all ItemGroups
            List<ItemGroupModel> result;
            using (var context = CreateDbContext())
            {
                var service = new ItemGroupService(context);
                result = await service.GetAll();
            }

            // Assert: Verify that the correct number of ItemGroups is returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsItemGroup()
        {
            // Arrange: Add an ItemGroup to the database
            var itemGroup = new ItemGroupModel
            {
                id = 1,
                name = "Group A",
                description = "Description for Group A",
                itemtype_id = 1,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.ItemGroups.Add(itemGroup);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve the ItemGroup by ID
            ItemGroupModel result;
            using (var context = CreateDbContext())
            {
                var service = new ItemGroupService(context);
                result = await service.GetById(1);
            }

            // Assert: Verify that the ItemGroup was retrieved correctly
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Group A", result.name);
        }

        [Fact]
        public async Task Add_ValidItemGroup_CreatesItemGroup()
        {
            // Arrange: Create a new ItemGroup
            var newItemGroup = new ItemGroupModel
            {
                name = "Group C",
                description = "Description for Group C",
                itemtype_id = 3,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act: Add the new ItemGroup to the database
            using (var context = CreateDbContext())
            {
                var service = new ItemGroupService(context);
                await service.Add(newItemGroup);
            }

            // Assert: Verify that the ItemGroup is added to the database
            using (var context = CreateDbContext())
            {
                var result = await context.ItemGroups.FirstOrDefaultAsync(i => i.name == "Group C");
                Assert.NotNull(result);
                Assert.Equal("Group C", result.name);
            }
        }

        [Fact]
        public async Task Update_ValidItemGroup_UpdatesItemGroup()
        {
            // Arrange: Add an ItemGroup to the database
            var itemGroup = new ItemGroupModel
            {
                id = 1,
                name = "Group A",
                description = "Description for Group A",
                itemtype_id = 1,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.ItemGroups.Add(itemGroup);
                await context.SaveChangesAsync();
            }

            // Act: Update the ItemGroup
            var updatedItemGroup = new ItemGroupModel
            {
                name = "Updated Group A",
                description = "Updated description for Group A",
                itemtype_id = 1,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                var service = new ItemGroupService(context);
                var result = await service.Update(1, updatedItemGroup);
            }

            // Assert: Verify that the ItemGroup was updated correctly
            using (var context = CreateDbContext())
            {
                var result = await context.ItemGroups.FirstOrDefaultAsync(i => i.id == 1);
                Assert.NotNull(result);
                Assert.Equal("Updated Group A", result.name);
            }
        }

        [Fact]
        public async Task Delete_ValidItemGroup_DeletesItemGroup()
        {
            // Arrange: Add an ItemGroup to the database
            var itemGroup = new ItemGroupModel
            {
                id = 1,
                name = "Group A",
                description = "Description for Group A",
                itemtype_id = 1,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.ItemGroups.Add(itemGroup);
                await context.SaveChangesAsync();
            }

            // Act: Delete the ItemGroup
            using (var context = CreateDbContext())
            {
                var service = new ItemGroupService(context);
                await service.Delete(1);
            }

            // Assert: Verify that the ItemGroup was deleted
            using (var context = CreateDbContext())
            {
                var result = await context.ItemGroups.FirstOrDefaultAsync(i => i.id == 1);
                Assert.Null(result); // The ItemGroup should be deleted
            }
        }
    }
}
