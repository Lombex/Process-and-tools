using CSharpAPI.Controllers;
using CSharpAPI.Models;
using CSharpAPI.Service;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Integration.Tests
{
    public class InventoriesControllerTest : IntegrationTestBase
    {
        private readonly InventoriesController _controller;
        private readonly IInventoriesService _service;

        public InventoriesControllerTest()
        {
            _service = new InventoriesService(DbContext);
            _controller = new InventoriesController(_service);
            SeedTestData();
        }

        private async void SeedTestData()
        {
            // Seed test inventory data
            var inventories = new List<InventorieModel>
            {
                new InventorieModel
                {
                    id = 1,
                    item_id = "ITEM001",
                    description = "Test Item 1",
                    item_reference = "REF001",
                    locations = new List<int> { 1, 2 },
                    total_on_hand = 100,
                    total_expected = 150,
                    total_ordered = 50,
                    total_allocated = 30,
                    total_available = 70,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new InventorieModel
                {
                    id = 2,
                    item_id = "ITEM002",
                    description = "Test Item 2",
                    item_reference = "REF002",
                    locations = new List<int> { 2, 3 },
                    total_on_hand = 200,
                    total_expected = 250,
                    total_ordered = 75,
                    total_allocated = 50,
                    total_available = 150,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.Inventors.AddRangeAsync(inventories);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllInventories_ReturnsAllInventories()
        {
            // Act
            var actionResult = await _controller.GetAllInventories();
            var result = actionResult.Result as OkObjectResult;
            var inventories = result?.Value as List<InventorieModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            inventories.Should().HaveCount(2);
            inventories.Should().Contain(i => i.item_id == "ITEM001");
            inventories.Should().Contain(i => i.item_id == "ITEM002");
        }

        [Fact]
        public async Task GetInventoryById_ReturnsInventory()
        {
            // Act
            var actionResult = await _controller.GetInventoryById(1);
            var result = actionResult.Result as OkObjectResult;
            var inventory = result?.Value as InventorieModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            inventory.Should().NotBeNull();
            inventory.item_id.Should().Be("ITEM001");
            inventory.description.Should().Be("Test Item 1");
            inventory.locations.Should().Contain(1);
            inventory.locations.Should().Contain(2);
        }

        [Fact]
        public async Task GetInventoryById_ReturnsNotFound_ForInvalidId()
        {
            // Act
            var actionResult = await _controller.GetInventoryById(999);
            var result = actionResult.Result as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);
            result.Value.As<string>().Should().Contain("Inventory with id 999 not found");
        }

        [Fact]
        public async Task CreateInventory_AddsNewInventory()
        {
            // Arrange
            var newInventory = new InventorieModel
            {
                item_id = "ITEM003",
                description = "Test Item 3",
                item_reference = "REF003",
                locations = new List<int> { 1 },
                total_on_hand = 50,
                total_expected = 100,
                total_ordered = 25,
                total_allocated = 10,
                total_available = 40
            };

            // Act
            var actionResult = await _controller.CreateInventory(newInventory);
            var result = actionResult.Result as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdInventory = result.Value as InventorieModel;
            createdInventory.Should().NotBeNull();
            createdInventory.item_id.Should().Be("ITEM003");
            createdInventory.description.Should().Be("Test Item 3");
        }

        [Fact]
        public async Task CreateInventory_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var actionResult = await _controller.CreateInventory(null);
            var result = actionResult.Result as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
            result.Value.Should().Be("Inventory data is null.");
        }

        [Fact]
        public async Task UpdateInventory_UpdatesExistingInventory()
        {
            // Arrange
            var updateInventory = new InventorieModel
            {
                item_id = "ITEM001-UPD",
                description = "Updated Test Item 1",
                item_reference = "REF001-UPD",
                locations = new List<int> { 1, 2, 3 },
                total_on_hand = 150,
                total_expected = 200,
                total_ordered = 75,
                total_allocated = 40,
                total_available = 110
            };

            // Act
            var result = await _controller.UpdateInventory(1, updateInventory) as IActionResult;

            // Assert
            result.Should().NotBeNull();
            (result as NoContentResult).StatusCode.Should().Be(204);

            // Verify the update
            var updatedInventory = await _service.GetInventoryById(1);
            updatedInventory.item_id.Should().Be("ITEM001-UPD");
            updatedInventory.description.Should().Be("Updated Test Item 1");
            updatedInventory.total_on_hand.Should().Be(150);
        }

        [Fact]
        public async Task UpdateInventory_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.UpdateInventory(1, null) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
            result.Value.Should().Be("Invalid inventory data.");
        }

        [Fact]
        public async Task UpdateInventory_ReturnsNotFound_ForInvalidId()
        {
            // Arrange
            var updateInventory = new InventorieModel
            {
                item_id = "INVALID",
                description = "Invalid Item"
            };

            // Act
            var result = await _controller.UpdateInventory(999, updateInventory) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);
            result.Value.Should().Be("Inventory with id 999 not found.");
        }

        [Fact]
        public async Task DeleteInventory_RemovesInventory()
        {
            // Act
            var result = await _controller.DeleteInventory(1) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);

            // Verify deletion
            var allInventories = await _service.GetAllInventories();
            allInventories.Should().NotContain(i => i.id == 1);
        }

        [Fact]
        public async Task DeleteInventory_ReturnsNotFound_ForInvalidId()
        {
            // Act
            var result = await _controller.DeleteInventory(999) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);
            result.Value.Should().Be("Inventory with id 999 not found.");
        }

        [Fact]
        public async Task GetInventoriesByItemId_ReturnsInventories()
        {
            // Act
            var actionResult = await _controller.GetInventoriesByItemId("ITEM001");
            var result = actionResult.Result as OkObjectResult;
            var inventories = result?.Value as List<InventorieModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            inventories.Should().NotBeNull();
            inventories.Should().AllSatisfy(i => i.item_id.Should().Be("ITEM001"));
        }

        [Fact]
        public async Task GetInventoriesByLocation_ReturnsInventories()
        {
            // Act
            var actionResult = await _controller.GetInventoriesByLocation(2);
            var result = actionResult.Result as OkObjectResult;
            var inventories = result?.Value as List<InventorieModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            inventories.Should().NotBeNull();
            inventories.Should().AllSatisfy(i => i.locations.Should().Contain(2));
        }
    }
}