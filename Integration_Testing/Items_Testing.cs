using CSharpAPI.Controllers;
using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class ItemControllerTest
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
                    code = "sjQ23408K",
                    description = "Face-to-face clear-thinking complexity",
                    short_description = "must",
                    upc_code = "6523540947122",
                    model_number = "63-OFFTq0T",
                    commodity_code = "oTo304",
                    item_line = 11,
                    item_group = 73,
                    item_type = 14,
                    unit_purchase_quantity = 47,
                    unit_order_quantity = 13,
                    pack_order_quantity = 11,
                    supplier_id = 34,
                    supplier_code = "SUP423",
                    supplier_part_number = "E-86805-uTM",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new ItemModel
                {
                    uid = "P000002",
                    code = "nyg48736S",
                    description = "Focused transitional alliance",
                    short_description = "may",
                    upc_code = "9733132830047",
                    model_number = "ck-109684-VFb",
                    commodity_code = "y-20588-owy",
                    item_line = 69,
                    item_group = 85,
                    item_type = 39,
                    unit_purchase_quantity = 10,
                    unit_order_quantity = 15,
                    pack_order_quantity = 23,
                    supplier_id = 57,
                    supplier_code = "SUP312",
                    supplier_part_number = "j-10730-ESk",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new ItemModel
                {
                    uid = "P000003",
                    code = "QVm03739H",
                    description = "Cloned actuating artificial intelligence",
                    short_description = "we",
                    upc_code = "3722576017240",
                    model_number = "aHx-68Q4",
                    commodity_code = "t-541-F0g",
                    item_line = 54,
                    item_group = 88,
                    item_type = 42,
                    unit_purchase_quantity = 30,
                    unit_order_quantity = 17,
                    pack_order_quantity = 11,
                    supplier_id = 2,
                    supplier_code = "SUP237",
                    supplier_part_number = "r-920-z2C",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAll_ReturnAllItems()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var result = await controller.GetAllItems() as OkObjectResult;
            var items = result?.Value as List<ItemModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(items);
        }

        [Fact]
        public async Task GetById_ReturnsItem()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var result = await controller.GetItemById("P000001") as OkObjectResult;
            var item = result?.Value as ItemModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(item);
            Xunit.Assert.Equal("sjQ23408K", item.code);
        }

        [Fact]
        public async Task CreateItem_ReturnsCreatedItem()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var newItem = new ItemModel
            {
                uid = "P000002",
                code = "newCode123",
                description = "New item description",
                short_description = "short",
                upc_code = "1234567890123",
                model_number = "MODEL123",
                commodity_code = "C-123",
                item_line = 10,
                item_group = 20,
                item_type = 30,
                supplier_id = 50,
                supplier_code = "SUP500",
                supplier_part_number = "PART500",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var result = await controller.CreateItem(newItem) as CreatedAtActionResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(newItem.uid, (result.Value as ItemModel).uid);
        }

        [Fact]
        public async Task UpdateItem_ReturnsUpdatedMessage()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var updatedItem = new ItemModel
            {
                uid = "P000001",
                code = "UpdatedCode",
                description = "Updated Description",
                short_description = "updated",
                upc_code = "9876543210123",
                model_number = "UPDATED_MODEL",
                commodity_code = "C-456",
                item_line = 15,
                item_group = 25,
                item_type = 35,
                supplier_id = 60,
                supplier_code = "SUP600",
                supplier_part_number = "PART600",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var result = await controller.UpdateItem("P000001", updatedItem) as OkObjectResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal("Item P000001 has been updated!", result.Value);
        }

        [Fact]
        public async Task DeleteItem_ReturnsDeletedMessage()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var result = await controller.DeleteItem("P000001") as OkObjectResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal("Item has been deleted!", result.Value);
        }

        [Fact]
        public async Task GetItemsByLineId_ReturnsItems()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var result = await controller.GetItemsByLineId(11) as OkObjectResult;
            var items = result?.Value as List<ItemModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(items);
            Xunit.Assert.Equal(11, items[0].item_line);
        }

        [Fact]
        public async Task GetItemsByGroupId_ReturnsItems()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var result = await controller.GetItemsByGroupId(73) as OkObjectResult;
            var items = result?.Value as List<ItemModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(items);
            Xunit.Assert.Equal(73, items[0].item_group);
        }

        [Fact]
        public async Task GetItemsBySupplierId_ReturnsItems()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ItemsService(dbContext);
            var controller = new ItemsController(service);

            var result = await controller.GetItemsBySupplierId(34) as OkObjectResult;
            var items = result?.Value as List<ItemModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(items);
            Xunit.Assert.Equal(34, items[0].supplier_id);
        }


    }
}