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
    public class ItemLinesControllerTest : IntegrationTestBase
    {
        private readonly ItemLinesController _controller;
        private readonly IItemLineService _service;
        private readonly IAuthService _authService;

        public ItemLinesControllerTest()
        {
            _service = new ItemLineService(DbContext);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new ItemLinesController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.ItemLine.RemoveRange(DbContext.ItemLine);
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var itemLines = new List<ItemLineModel>
            {
                new ItemLineModel
                {
                    name = "Line 1",
                    description = "Test Line 1",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemLineModel
                {
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
        public async Task GetAll_ReturnsAllItemLines_WhenAuthorized()
        {
            // Act
            var actionResult = await _controller.GetAll(0);
            
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
            var itemLinesProperty = responseType.GetProperty("ItemLine").GetValue(response) as IEnumerable<object>;

            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(2);
            totalPagesProperty.Should().Be(1);

            itemLinesProperty.Should().NotBeNull();
            var itemLines = itemLinesProperty.ToList();
            itemLines.Should().HaveCount(2);

            // Check first item line
            var firstItemLine = itemLines.First();
            var firstItemLineType = firstItemLine.GetType();
            (firstItemLineType.GetProperty("Name").GetValue(firstItemLine) as string).Should().Be("Line 1");
            
            // Check second item line
            var secondItemLine = itemLines.Last();
            var secondItemLineType = secondItemLine.GetType();
            (secondItemLineType.GetProperty("Name").GetValue(secondItemLine) as string).Should().Be("Line 2");
        }

        [Fact]
        public async Task GetAll_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            // Act
            var result = await _controller.GetAll(0);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetById_ReturnsItemLine_WhenAuthorized()
        {
            // Arrange
            var itemLine = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.name == "Line 1");
            itemLine.Should().NotBeNull();

            // Act
            var result = await _controller.GetById(itemLine.id) as OkObjectResult;
            var returnedItemLine = result?.Value as ItemLineModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedItemLine.Should().NotBeNull();
            returnedItemLine.name.Should().Be("Line 1");
            returnedItemLine.description.Should().Be("Test Line 1");
        }

        [Fact]
        public async Task GetById_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var itemLine = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.name == "Line 1");
            itemLine.Should().NotBeNull();

            // Act
            var result = await _controller.GetById(itemLine.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Create_AddsNewItemLine_WhenAuthorized()
        {
            // Arrange
            var newItemLine = new ItemLineModel
            {
                name = "New Line",
                description = "Test New Line",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act
            var result = await _controller.Create(newItemLine) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdItemLine = result.Value as ItemLineModel;
            createdItemLine.Should().NotBeNull();
            createdItemLine.name.Should().Be("New Line");
            createdItemLine.description.Should().Be("Test New Line");
        }

        [Fact]
        public async Task Create_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var newItemLine = new ItemLineModel
            {
                name = "New Line",
                description = "Test New Line"
            };

            // Act
            var result = await _controller.Create(newItemLine);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Update_UpdatesExistingItemLine_WhenAuthorized()
        {
            // Arrange
            var itemLine = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.name == "Line 1");
            itemLine.Should().NotBeNull();

            var updateItemLine = new ItemLineModel
            {
                name = "Updated Line",
                description = "Updated Description"
            };

            // Act
            var result = await _controller.Update(itemLine.id, updateItemLine) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be($"ItemLine {itemLine.id} has been updated!");

            var updatedItemLine = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.id == itemLine.id);
            updatedItemLine.Should().NotBeNull();
            updatedItemLine.name.Should().Be("Updated Line");
            updatedItemLine.description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task Update_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var itemLine = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.name == "Line 1");
            itemLine.Should().NotBeNull();

            var updateItemLine = new ItemLineModel
            {
                name = "Updated Line",
                description = "Updated Description"
            };

            // Act
            var result = await _controller.Update(itemLine.id, updateItemLine);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Delete_RemovesItemLine_WhenAuthorized()
        {
            // Arrange
            var itemLine = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.name == "Line 1");
            itemLine.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(itemLine.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("ItemLine has been deleted.");

            var deletedItemLine = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.id == itemLine.id);
            deletedItemLine.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var itemLine = await DbContext.ItemLine.FirstOrDefaultAsync(l => l.name == "Line 1");
            itemLine.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(itemLine.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }
    }
}
