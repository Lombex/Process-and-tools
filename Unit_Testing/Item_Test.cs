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
    public class ItemsServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public ItemsServiceTests()
        {
            // Set up In-Memory database options
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "ItemTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task GetAllItems_ReturnsListOfItems()
        {
            // Arrange: Create mock data for Items
            var itemList = new List<ItemModel>
            {
                new ItemModel
                {
                    uid = "P000001",
                    code = "A001",
                    description = "Item 1 Description",
                    short_description = "Item 1",
                    upc_code = "123456789",
                    model_number = "ABC123",
                    commodity_code = "X001",
                    item_line = 1,
                    item_group = 1,
                    item_type = 1,
                    unit_purchase_quantity = 10,
                    unit_order_quantity = 5,
                    pack_order_quantity = 2,
                    supplier_id = 1,
                    supplier_code = "S001",
                    supplier_part_number = "SP001",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemModel
                {
                    uid = "P000002",
                    code = "B001",
                    description = "Item 2 Description",
                    short_description = "Item 2",
                    upc_code = "987654321",
                    model_number = "XYZ987",
                    commodity_code = "X002",
                    item_line = 2,
                    item_group = 1,
                    item_type = 2,
                    unit_purchase_quantity = 20,
                    unit_order_quantity = 10,
                    pack_order_quantity = 5,
                    supplier_id = 2,
                    supplier_code = "S002",
                    supplier_part_number = "SP002",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            using (var context = CreateDbContext())
            {
                context.itemModels.AddRange(itemList);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve all items
            List<ItemModel> result;
            using (var context = CreateDbContext())
            {
                var service = new ItemsService(context);
                result = await service.GetAllItems();
            }

            // Assert: Verify that the correct number of items is returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetItemById_ValidUid_ReturnsItem()
        {
            // Arrange: Add an Item to the database
            var item = new ItemModel
            {
                uid = "P000001",
                code = "A001",
                description = "Item 1 Description",
                short_description = "Item 1",
                upc_code = "123456789",
                model_number = "ABC123",
                commodity_code = "X001",
                item_line = 1,
                item_group = 1,
                item_type = 1,
                unit_purchase_quantity = 10,
                unit_order_quantity = 5,
                pack_order_quantity = 2,
                supplier_id = 1,
                supplier_code = "S001",
                supplier_part_number = "SP001",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.itemModels.Add(item);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve the item by UID
            ItemModel result;
            using (var context = CreateDbContext())
            {
                var service = new ItemsService(context);
                result = await service.GetItemById("P000001");
            }

            // Assert: Verify that the item was retrieved correctly
            Assert.NotNull(result);
            Assert.Equal("P000001", result.uid);
            Assert.Equal("A001", result.code);
        }

        [Fact]
        public async Task CreateItem_ValidItem_CreatesItem()
        {
            // Arrange: Create a new Item
            var newItem = new ItemModel
            {
                code = "C001",
                description = "New Item Description",
                short_description = "New Item",
                upc_code = "1122334455",
                model_number = "DEF456",
                commodity_code = "X003",
                item_line = 3,
                item_group = 1,
                item_type = 1,
                unit_purchase_quantity = 30,
                unit_order_quantity = 15,
                pack_order_quantity = 10,
                supplier_id = 3,
                supplier_code = "S003",
                supplier_part_number = "SP003",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act: Add the new item to the database
            using (var context = CreateDbContext())
            {
                var service = new ItemsService(context);
                await service.CreateItem(newItem);
            }

            // Assert: Verify that the item is added to the database
            using (var context = CreateDbContext())
            {
                var result = await context.itemModels.FirstOrDefaultAsync(i => i.code == "C001");
                Assert.NotNull(result);
                Assert.Equal("C001", result.code);
            }
        }

        [Fact]
        public async Task UpdateItem_ValidItem_UpdatesItem()
        {
            // Arrange: Add an Item to the database
            var item = new ItemModel
            {
                uid = "P000001",
                code = "A001",
                description = "Item 1 Description",
                short_description = "Item 1",
                upc_code = "123456789",
                model_number = "ABC123",
                commodity_code = "X001",
                item_line = 1,
                item_group = 1,
                item_type = 1,
                unit_purchase_quantity = 10,
                unit_order_quantity = 5,
                pack_order_quantity = 2,
                supplier_id = 1,
                supplier_code = "S001",
                supplier_part_number = "SP001",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.itemModels.Add(item);
                await context.SaveChangesAsync();
            }

            // Act: Update the item
            var updatedItem = new ItemModel
            {
                uid = "P000001", // Keep the same UID for update
                code = "A002",
                description = "Updated Item Description",
                short_description = "Updated Item",
                upc_code = "987654321",
                model_number = "XYZ987",
                commodity_code = "X002",
                item_line = 2,
                item_group = 2,
                item_type = 2,
                unit_purchase_quantity = 20,
                unit_order_quantity = 10,
                pack_order_quantity = 5,
                supplier_id = 2,
                supplier_code = "S002",
                supplier_part_number = "SP002",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                var service = new ItemsService(context);
                await service.UpdateItem("P000001", updatedItem);
            }

            // Assert: Verify that the item was updated correctly
            using (var context = CreateDbContext())
            {
                var result = await context.itemModels.FirstOrDefaultAsync(i => i.uid == "P000001");
                Assert.NotNull(result);
                Assert.Equal("A002", result.code);
                Assert.Equal("Updated Item Description", result.description);
            }
        }

        [Fact]
        public async Task DeleteItem_ValidItem_DeletesItem()
        {
            // Arrange: Add an Item to the database
            var item = new ItemModel
            {
                uid = "P000001",
                code = "A001",
                description = "Item 1 Description",
                short_description = "Item 1",
                upc_code = "123456789",
                model_number = "ABC123",
                commodity_code = "X001",
                item_line = 1,
                item_group = 1,
                item_type = 1,
                unit_purchase_quantity = 10,
                unit_order_quantity = 5,
                pack_order_quantity = 2,
                supplier_id = 1,
                supplier_code = "S001",
                supplier_part_number = "SP001",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.itemModels.Add(item);
                await context.SaveChangesAsync();
            }

            // Act: Delete the item
            using (var context = CreateDbContext())
            {
                var service = new ItemsService(context);
                await service.DeleteItem("P000001");
            }

            // Assert: Verify that the item was deleted
            using (var context = CreateDbContext())
            {
                var result = await context.itemModels.FirstOrDefaultAsync(i => i.uid == "P000001");
                Assert.Null(result); // The item should be deleted
            }
        }
    }
}
