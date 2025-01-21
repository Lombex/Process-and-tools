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
    public class ItemTypesTest : IntegrationTestBase
    {
        private readonly ItemTypesController _controller;
        private readonly IItemTypeService _service;
        private readonly IAuthService _authService;

        public ItemTypesTest()
        {
            _service = new ItemTypeService(DbContext);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new ItemTypesController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.ItemType.RemoveRange(DbContext.ItemType);
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var itemTypes = new List<ItemTypeModel>
            {
                new ItemTypeModel
                {
                    name = "Type1",
                    description = "Description1",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemTypeModel
                {
                    name = "Type2",
                    description = "Description2",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemTypeModel
                {
                    name = "Type3",
                    description = "Description3",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.ItemType.AddRangeAsync(itemTypes);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_ReturnsAllItemTypes_WhenAuthorized()
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
            var itemTypesProperty = responseType.GetProperty("ItemType").GetValue(response) as IEnumerable<object>;

            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(3);
            totalPagesProperty.Should().Be(1);

            itemTypesProperty.Should().NotBeNull();
            var itemTypes = itemTypesProperty.ToList();
            itemTypes.Should().HaveCount(3);

            // Check specific item types
            var firstItemType = itemTypes.First();
            var firstItemTypeType = firstItemType.GetType();
            (firstItemTypeType.GetProperty("Name").GetValue(firstItemType) as string).Should().Be("Type1");
            
            var lastItemType = itemTypes.Last();
            var lastItemTypeType = lastItemType.GetType();
            (lastItemTypeType.GetProperty("Name").GetValue(lastItemType) as string).Should().Be("Type3");
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
        public async Task GetById_ReturnsItemType_WhenAuthorized()
        {
            // Arrange
            var itemType = await DbContext.ItemType.FirstOrDefaultAsync(it => it.name == "Type1");
            itemType.Should().NotBeNull();

            // Act
            var result = await _controller.GetById(itemType.id) as OkObjectResult;
            var returnedItemType = result?.Value as ItemTypeModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedItemType.Should().NotBeNull();
            returnedItemType.name.Should().Be("Type1");
            returnedItemType.description.Should().Be("Description1");
        }

        [Fact]
        public async Task GetById_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var itemType = await DbContext.ItemType.FirstOrDefaultAsync(it => it.name == "Type1");
            itemType.Should().NotBeNull();

            // Act
            var result = await _controller.GetById(itemType.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Create_AddsNewItemType_WhenAuthorized()
        {
            // Arrange
            var newItemType = new ItemTypeModel
            {
                name = "NewType",
                description = "NewDescription",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act
            var result = await _controller.Create(newItemType) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdItemType = result.Value as ItemTypeModel;
            createdItemType.Should().NotBeNull();
            createdItemType.name.Should().Be("NewType");
            createdItemType.description.Should().Be("NewDescription");
        }

        [Fact]
        public async Task Create_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var newItemType = new ItemTypeModel
            {
                name = "NewType",
                description = "NewDescription"
            };

            // Act
            var result = await _controller.Create(newItemType);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Update_UpdatesExistingItemType_WhenAuthorized()
        {
            // Arrange
            var itemType = await DbContext.ItemType.FirstOrDefaultAsync(it => it.name == "Type1");
            itemType.Should().NotBeNull();

            var updateItemType = new ItemTypeModel
            {
                name = "UpdatedType1",
                description = "UpdatedDescription1"
            };

            // Act
            var result = await _controller.Update(itemType.id, updateItemType) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().BeEquivalentTo(new { message = "ItemTypes has been updated!" });

            var updatedItemType = await DbContext.ItemType.FirstOrDefaultAsync(it => it.id == itemType.id);
            updatedItemType.Should().NotBeNull();
            updatedItemType.name.Should().Be("UpdatedType1");
            updatedItemType.description.Should().Be("UpdatedDescription1");
        }

        [Fact]
        public async Task Update_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var itemType = await DbContext.ItemType.FirstOrDefaultAsync(it => it.name == "Type1");
            itemType.Should().NotBeNull();

            var updateItemType = new ItemTypeModel
            {
                name = "UpdatedType1",
                description = "UpdatedDescription1"
            };

            // Act
            var result = await _controller.Update(itemType.id, updateItemType);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Delete_RemovesItemType_WhenAuthorized()
        {
            // Arrange
            var itemType = await DbContext.ItemType.FirstOrDefaultAsync(it => it.name == "Type1");
            itemType.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(itemType.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().BeEquivalentTo(new { message = "Itemtype has been deleted!" });

            var deletedItemType = await DbContext.ItemType.FirstOrDefaultAsync(it => it.id == itemType.id);
            deletedItemType.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var itemType = await DbContext.ItemType.FirstOrDefaultAsync(it => it.name == "Type1");
            itemType.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(itemType.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }
    }
}