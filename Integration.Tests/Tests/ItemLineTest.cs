using CSharpAPI.Controllers;
using CSharpAPI.Models;
using CSharpAPI.Service;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Integration.Tests.Tests
{
    public class ItemLinesControllerTest : IntegrationTestBase
    {
        private readonly ItemLinesController _controller;
        private readonly IItemLineService _service;

        public ItemLinesControllerTest()
        {
            _service = new ItemLineService(DbContext);
            _controller = new ItemLinesController(_service);

            SeedTestData();
        }

        private async void SeedTestData()
        {
            var itemLines = new List<ItemLineModel>
            {
                new ItemLineModel
                {
                    id = 1,
                    name = "Line 1",
                    description = "Test Line 1",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemLineModel
                {
                    id = 2,
                    name = "Line 2",
                    description = "Test Line 2",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.ItemLine.AddRangeAsync(itemLines);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_ReturnsAllItemLines()
        {
            // Act
            var result = await _controller.GetAll() as OkObjectResult;
            var itemLines = result?.Value as IEnumerable<ItemLineModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            itemLines.Should().HaveCount(2);
            itemLines.Should().Contain(l => l.name == "Line 1");
            itemLines.Should().Contain(l => l.name == "Line 2");
        }

        [Fact]
        public async Task GetById_ReturnsItemLine()
        {
            // Act
            var result = await _controller.GetById(1) as OkObjectResult;
            var itemLine = result?.Value as ItemLineModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            itemLine.Should().NotBeNull();
            itemLine.name.Should().Be("Line 1");
            itemLine.description.Should().Be("Test Line 1");
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_ForInvalidId()
        {
            // Act
            var result = await _controller.GetById(999) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);
            result.Value.Should().Be("ItemLine with id 999 not found.");
        }

        [Fact]
        public async Task Create_AddsNewItemLine()
        {
            // Arrange
            var newItemLine = new ItemLineModel
            {
                name = "New Line",
                description = "Test New Line"
            };

            // Act
            var result = await _controller.Create(newItemLine) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdItemLine = result.Value as ItemLineModel;
            createdItemLine.Should().NotBeNull();
            createdItemLine.name.Should().Be("New Line");
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.Create(null) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
            result.Value.Should().Be("Request is empty!");
        }

        [Fact]
        public async Task Update_UpdatesExistingItemLine()
        {
            // Arrange
            var updateItemLine = new ItemLineModel
            {
                name = "Updated Line",
                description = "Updated Description"
            };

            // Act
            var result = await _controller.Update(1, updateItemLine) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("ItemLine 1 has been updated!");
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.Update(1, null) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
            result.Value.Should().Be("Request is empty!");
        }

        [Fact]
        public async Task Update_ReturnsNotFound_ForInvalidId()
        {
            // Arrange
            var updateItemLine = new ItemLineModel
            {
                name = "Updated Line",
                description = "Updated Description"
            };

            // Act
            var result = await _controller.Update(999, updateItemLine) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);
            result.Value.Should().Be("ItemLine with id 999 not found.");
        }

        [Fact]
        public async Task Delete_RemovesItemLine()
        {
            // Act
            var result = await _controller.Delete(1) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("ItemLine has been deleted.");

            var allItemLines = await _service.GetAllItemLines();
            allItemLines.Should().NotContain(i => i.id == 1);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_ForInvalidId()
        {
            // Act
            var result = await _controller.Delete(999) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);
            result.Value.Should().Be("ItemLine with id 999 not found!");
        }
    }
}