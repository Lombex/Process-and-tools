using CSharpAPI.Controllers;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Data;
using CSharpAPI.Services.Auth;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integration.Tests.Tests
{
    public class ItemsControllerTest : IntegrationTestBase
    {
        private readonly ItemsController _controller;
        private readonly IItemsService _service;
        private readonly IAuthService _authService;

        public ItemsControllerTest()
        {
            _service = new ItemsService(DbContext, HistoryService);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new ItemsController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.itemModels.RemoveRange(DbContext.itemModels);
            DbContext.ItemLine.RemoveRange(DbContext.ItemLine);
            DbContext.ItemGroups.RemoveRange(DbContext.ItemGroups);
            DbContext.ItemType.RemoveRange(DbContext.ItemType);
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            // Add item line and group prerequisites
            var itemLine = new ItemLineModel 
            { 
                name = "Test Line",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await DbContext.ItemLine.AddAsync(itemLine);
            
            var itemGroup = new ItemGroupModel 
            { 
                name = "Test Group",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await DbContext.ItemGroups.AddAsync(itemGroup);
            
            var itemType = new ItemTypeModel 
            { 
                name = "Test Type",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await DbContext.ItemType.AddAsync(itemType);
            
            await DbContext.SaveChangesAsync();

            // Get the IDs after they're saved
            var line = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.name == "Test Line");
            var group = await DbContext.ItemGroups.FirstOrDefaultAsync(g => g.name == "Test Group");
            var type = await DbContext.ItemType.FirstOrDefaultAsync(t => t.name == "Test Type");

            // Seed test items
            var items = new List<ItemModel>
            {
                new ItemModel
                {
                    uid = "P000001",
                    code = "ITEM001",
                    description = "Test Item 1",
                    short_description = "Test 1",
                    item_line = line.id,
                    item_group = group.id,
                    item_type = type.id,
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
                    item_line = line.id,
                    item_group = group.id,
                    item_type = type.id,
                    supplier_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.itemModels.AddRangeAsync(items);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllItems_ReturnsAllItems_WhenAuthorized()
        {
            // Act
            var actionResult = await _controller.GetAllItems(0);
            
            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            
            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var response = okResult.Value as object;
            response.Should().NotBeNull();
            
            var responseType = response.GetType();
            var pageProperty = responseType.GetProperty("Page").GetValue(response) as int?;
            var pageSizeProperty = responseType.GetProperty("PageSize").GetValue(response) as int?;
            var totalItemsProperty = responseType.GetProperty("TotalItems").GetValue(response) as int?;
            var totalPagesProperty = responseType.GetProperty("TotalPages").GetValue(response) as int?;
            var itemsProperty = responseType.GetProperty("Items").GetValue(response) as IEnumerable<object>;

            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(2);
            totalPagesProperty.Should().Be(1);

            itemsProperty.Should().NotBeNull();
            var items = itemsProperty.ToList();
            items.Should().HaveCount(2);

            // Check first item
            var firstItem = items.First();
            var firstItemType = firstItem.GetType();
            (firstItemType.GetProperty("Code").GetValue(firstItem) as string).Should().Be("ITEM001");
            
            // Check second item
            var secondItem = items.Last();
            var secondItemType = secondItem.GetType();
            (secondItemType.GetProperty("Code").GetValue(secondItem) as string).Should().Be("ITEM002");
        }

        [Fact]
        public async Task GetAllItems_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            // Act
            var result = await _controller.GetAllItems(0);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetItemById_ReturnsItem_WhenAuthorized()
        {
            // Arrange
            var item = await DbContext.itemModels.FirstOrDefaultAsync(i => i.code == "ITEM001");
            item.Should().NotBeNull();

            // Act
            var result = await _controller.GetItemById(item.uid) as OkObjectResult;
            var returnedItem = result?.Value as ItemModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedItem.Should().NotBeNull();
            returnedItem.code.Should().Be("ITEM001");
            returnedItem.description.Should().Be("Test Item 1");
        }

        [Fact]
        public async Task GetItemById_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var item = await DbContext.itemModels.FirstOrDefaultAsync(i => i.code == "ITEM001");
            item.Should().NotBeNull();

            // Act
            var result = await _controller.GetItemById(item.uid);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task CreateItem_AddsNewItem_WhenAuthorized()
        {
            // Arrange
            var line = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.name == "Test Line");
            var group = await DbContext.ItemGroups.FirstOrDefaultAsync(g => g.name == "Test Group");
            var type = await DbContext.ItemType.FirstOrDefaultAsync(t => t.name == "Test Type");

            var newItem = new ItemModel
            {
                code = "ITEM003",
                description = "Test Item 3",
                short_description = "Test 3",
                item_line = line.id,
                item_group = group.id,
                item_type = type.id,
                supplier_id = 1,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act
            var result = await _controller.CreateItem(newItem) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdItem = result.Value as ItemModel;
            createdItem.Should().NotBeNull();
            createdItem.code.Should().Be("ITEM003");
            createdItem.description.Should().Be("Test Item 3");
        }

        [Fact]
        public async Task CreateItem_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var line = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.name == "Test Line");
            var group = await DbContext.ItemGroups.FirstOrDefaultAsync(g => g.name == "Test Group");
            var type = await DbContext.ItemType.FirstOrDefaultAsync(t => t.name == "Test Type");

            var newItem = new ItemModel
            {
                code = "ITEM003",
                description = "Test Item 3",
                item_line = line.id,
                item_group = group.id,
                item_type = type.id
            };

            // Act
            var result = await _controller.CreateItem(newItem);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task UpdateItem_UpdatesExistingItem_WhenAuthorized()
        {
            // Arrange
            var item = await DbContext.itemModels.FirstOrDefaultAsync(i => i.code == "ITEM001");
            item.Should().NotBeNull();

            var updateItem = new ItemModel
            {
                code = "ITEM001-UPD",
                description = "Updated Item",
                short_description = "Updated"
            };

            // Act
            var result = await _controller.UpdateItem(item.uid, updateItem) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be($"Item {item.uid} has been updated!");

            var updatedItem = await DbContext.itemModels.FirstOrDefaultAsync(i => i.uid == item.uid);
            updatedItem.Should().NotBeNull();
            updatedItem.code.Should().Be("ITEM001-UPD");
            updatedItem.description.Should().Be("Updated Item");
        }

        [Fact]
        public async Task UpdateItem_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var item = await DbContext.itemModels.FirstOrDefaultAsync(i => i.code == "ITEM001");
            item.Should().NotBeNull();

            var updateItem = new ItemModel
            {
                code = "ITEM001-UPD",
                description = "Updated Item"
            };

            // Act
            var result = await _controller.UpdateItem(item.uid, updateItem);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task DeleteItem_RemovesItem_WhenAuthorized()
        {
            // Arrange
            var item = await DbContext.itemModels.FirstOrDefaultAsync(i => i.code == "ITEM001");
            item.Should().NotBeNull();

            // Act
            var result = await _controller.DeleteItem(item.uid) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Item has been deleted!");

            var deletedItem = await DbContext.itemModels.FirstOrDefaultAsync(i => i.uid == item.uid);
            deletedItem.Should().BeNull();
        }

        [Fact]
        public async Task DeleteItem_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var item = await DbContext.itemModels.FirstOrDefaultAsync(i => i.code == "ITEM001");
            item.Should().NotBeNull();

            // Act
            var result = await _controller.DeleteItem(item.uid);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetItemsByLineId_ReturnsItems_WhenAuthorized()
        {
            // Arrange
            var line = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.name == "Test Line");
            line.Should().NotBeNull();

            // Act
            var result = await _controller.GetItemsByLineId(line.id) as OkObjectResult;
            var items = result?.Value as IEnumerable<ItemModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            items.Should().NotBeNull();
            items.Should().OnlyContain(i => i.item_line == line.id);
        }

        [Fact]
        public async Task GetItemsByLineId_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var line = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.name == "Test Line");
            line.Should().NotBeNull();

            // Act
            var result = await _controller.GetItemsByLineId(line.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetItemsByGroupId_ReturnsItems_WhenAuthorized()
        {
            // Arrange
            var group = await DbContext.ItemGroups.FirstOrDefaultAsync(g => g.name == "Test Group");
            group.Should().NotBeNull();

            // Act
            var result = await _controller.GetItemsByGroupId(group.id) as OkObjectResult;
            var items = result?.Value as IEnumerable<ItemModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            items.Should().NotBeNull();
            items.Should().OnlyContain(i => i.item_group == group.id);
        }

        [Fact]
        public async Task GetItemsByGroupId_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var group = await DbContext.ItemGroups.FirstOrDefaultAsync(g => g.name == "Test Group");
            group.Should().NotBeNull();

            // Act
            var result = await _controller.GetItemsByGroupId(group.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetItemsByTypeId_ReturnsItems_WhenAuthorized()
        {
            // Arrange
            var type = await DbContext.ItemType.FirstOrDefaultAsync(t => t.name == "Test Type");
            type.Should().NotBeNull();

            // Act
            var result = await _controller.GetItemsByTypeId(type.id) as OkObjectResult;
            var items = result?.Value as IEnumerable<ItemModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            items.Should().NotBeNull();
            items.Should().OnlyContain(i => i.item_type == type.id);
        }

        [Fact]
        public async Task GetItemsByTypeId_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var type = await DbContext.ItemType.FirstOrDefaultAsync(t => t.name == "Test Type");
            type.Should().NotBeNull();

            // Act
            var result = await _controller.GetItemsByTypeId(type.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }
    }
}
