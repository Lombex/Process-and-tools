using CSharpAPI.Controllers;
using CSharpAPI.Models;
using CSharpAPI.Service;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Integration.Tests.Tests
{
    public class ItemsControllerTest : IntegrationTestBase
    {
        private readonly ItemsController _controller;
        private readonly IItemsService _service;

        public ItemsControllerTest()
        {
            _service = new ItemsService(DbContext);
            _controller = new ItemsController(_service);

            SeedTestData();
        }

        private async void SeedTestData()
        {
            // Seed test items
            var items = new List<ItemModel>
            {
                new ItemModel
                {
                    uid = "P000001",
                    code = "ITEM001",
                    description = "Test Item 1",
                    short_description = "Test 1",
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
                    description = "Test Item 2",
                    short_description = "Test 2",
                    item_line = 2,
                    item_group = 2,
                    item_type = 1,
                    supplier_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.itemModels.AddRangeAsync(items);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllItems_ReturnsAllItems()
        {
            // Act
            var result = await _controller.GetAllItems() as OkObjectResult;
            var items = result?.Value as List<ItemModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            items.Should().HaveCount(2);
            items.Should().Contain(i => i.code == "ITEM001");
            items.Should().Contain(i => i.code == "ITEM002");
        }

        [Fact]
        public async Task GetItemById_ReturnsItem()
        {
            // Act
            var result = await _controller.GetItemById("P000001") as OkObjectResult;
            var item = result?.Value as ItemModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            item.Should().NotBeNull();
            item.code.Should().Be("ITEM001");
        }

        [Fact]
        public async Task GetItemById_ThrowsException_ForInvalidId()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.GetItemById("INVALID");
            });

            exception.Message.Should().Be("Item not found!");
        }

        [Fact]
        public async Task CreateItem_AddsNewItem()
        {
            // Arrange
            var newItem = new ItemModel
            {
                code = "ITEM003",
                description = "Test Item 3",
                short_description = "Test 3",
                item_line = 1,
                item_group = 1,
                item_type = 1
            };

            // Act
            var result = await _controller.CreateItem(newItem) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            result.ActionName.Should().Be(nameof(ItemsController.GetItemById));
            
            var createdItem = result.Value as ItemModel;
            createdItem.Should().NotBeNull();
            createdItem.code.Should().Be("ITEM003");
        }

        [Fact]
        public async Task CreateItem_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.CreateItem(null) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
            result.Value.Should().Be("Request is empty!");
        }

        [Fact]
        public async Task UpdateItem_UpdatesExistingItem()
        {
            // Arrange
            var updatedItem = new ItemModel
            {
                code = "ITEM001-UPD",
                description = "Updated Item",
                short_description = "Updated"
            };

            // Act
            var result = await _controller.UpdateItem("P000001", updatedItem) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Item P000001 has been updated!");
        }

        [Fact]
        public async Task UpdateItem_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.UpdateItem("P000001", null) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
            result.Value.Should().Be("Request is empty!");
        }

        [Fact]
        public async Task UpdateItem_ThrowsException_ForInvalidId()
        {
            // Arrange
            var updatedItem = new ItemModel
            {
                code = "INVALID-UPD",
                description = "Invalid Item"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.UpdateItem("INVALID", updatedItem);
            });

            exception.Message.Should().Be("Item not found!");
        }

        [Fact]
        public async Task DeleteItem_RemovesItem()
        {
            // Act
            var result = await _controller.DeleteItem("P000001") as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Item has been deleted!");
        }

        [Fact]
        public async Task DeleteItem_ThrowsException_ForInvalidId()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.DeleteItem("INVALID");
            });

            exception.Message.Should().Be("Item not found!");
        }

        [Fact]
        public async Task GetItemsByLineId_ReturnsItems()
        {
            // Act
            var result = await _controller.GetItemsByLineId(1) as OkObjectResult;
            var items = result?.Value as IEnumerable<ItemModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            items.Should().NotBeNull();
            items.Should().Contain(i => i.item_line == 1);
        }

        [Fact]
        public async Task GetItemsByGroupId_ReturnsItems()
        {
            // Act
            var result = await _controller.GetItemsByGroupId(1) as OkObjectResult;
            var items = result?.Value as IEnumerable<ItemModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            items.Should().NotBeNull();
            items.Should().Contain(i => i.item_group == 1);
        }

        [Fact]
        public async Task GetItemsBySupplierId_ReturnsItems()
        {
            // Act
            var result = await _controller.GetItemsBySupplierId(1) as OkObjectResult;
            var items = result?.Value as IEnumerable<ItemModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            items.Should().NotBeNull();
            items.Should().Contain(i => i.supplier_id == 1);
        }
    }
}