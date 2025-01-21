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
    public class ItemGroupsTest : IntegrationTestBase
    {
        private readonly ItemGroupsController _controller;
        private readonly IItemGroupService _service;
        private readonly IAuthService _authService;

        public ItemGroupsTest()
        {
            _service = new ItemGroupService(DbContext);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new ItemGroupsController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.ItemGroups.RemoveRange(DbContext.ItemGroups);
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var itemGroups = new List<ItemGroupModel>
            {
                new ItemGroupModel
                {
                    name = "Group1",
                    description = "Description1",
                    itemtype_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemGroupModel
                {
                    name = "Group2",
                    description = "Description2",
                    itemtype_id = 2,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.ItemGroups.AddRangeAsync(itemGroups);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_ReturnsAllItemGroups_WhenAuthorized()
        {
            // Act
            var actionResult = await _controller.GetAll(0);
            
            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            
            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(200);

            var response = okResult?.Value as object;
            response.Should().NotBeNull();
            
            var responseType = response?.GetType();
            var pageProperty = responseType?.GetProperty("Page")?.GetValue(response) as int?;
            var pageSizeProperty = responseType?.GetProperty("PageSize")?.GetValue(response) as int?;
            var totalItemsProperty = responseType?.GetProperty("TotalItems")?.GetValue(response) as int?;
            var totalPagesProperty = responseType?.GetProperty("TotalPages")?.GetValue(response) as int?;
            var itemGroupsProperty = responseType?.GetProperty("ItemGroup")?.GetValue(response) as IEnumerable<object>;

            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(2);
            totalPagesProperty.Should().Be(1);

            itemGroupsProperty.Should().NotBeNull();
            var itemGroups = itemGroupsProperty?.ToList();
            itemGroups.Should().HaveCount(2);

            // Check first item group
            var firstItemGroup = itemGroups?.First();
            var firstItemGroupType = firstItemGroup?.GetType();
            (firstItemGroupType?.GetProperty("Name")?.GetValue(firstItemGroup) as string).Should().Be("Group1");
            
            // Check second item group
            var secondItemGroup = itemGroups?.Last();
            var secondItemGroupType = secondItemGroup?.GetType();
            (secondItemGroupType?.GetProperty("Name")?.GetValue(secondItemGroup) as string).Should().Be("Group2");
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
        public async Task GetById_ReturnsItemGroup_WhenAuthorized()
        {
            // Arrange
            var itemGroup = await DbContext.ItemGroups.FirstOrDefaultAsync(ig => ig.name == "Group1");
            itemGroup.Should().NotBeNull();

            // Act
            var result = await _controller.GetById(itemGroup.id) as OkObjectResult;
            var returnedItemGroup = result?.Value as ItemGroupModel;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(200);
            returnedItemGroup.Should().NotBeNull();
            returnedItemGroup?.name.Should().Be("Group1");
            returnedItemGroup?.description.Should().Be("Description1");
        }

        [Fact]
        public async Task GetById_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var itemGroup = await DbContext.ItemGroups.FirstOrDefaultAsync(ig => ig.name == "Group1");
            itemGroup.Should().NotBeNull();

            // Act
            var result = await _controller.GetById(itemGroup.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Create_AddsNewItemGroup_WhenAuthorized()
        {
            // Arrange
            var newItemGroup = new ItemGroupModel
            {
                name = "New Group",
                description = "Test New Group",
                itemtype_id = 3
            };

            // Act
            var result = await _controller.Create(newItemGroup) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(201);
            var createdItemGroup = result?.Value as ItemGroupModel;
            createdItemGroup.Should().NotBeNull();
            createdItemGroup?.name.Should().Be("New Group");
            createdItemGroup?.description.Should().Be("Test New Group");
            createdItemGroup?.itemtype_id.Should().Be(3);
        }

        [Fact]
        public async Task Create_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var newItemGroup = new ItemGroupModel
            {
                name = "New Group",
                description = "Test New Group",
                itemtype_id = 3
            };

            // Act
            var result = await _controller.Create(newItemGroup);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Update_UpdatesExistingItemGroup_WhenAuthorized()
        {
            // Arrange
            var itemGroup = await DbContext.ItemGroups.FirstOrDefaultAsync(ig => ig.name == "Group1");
            itemGroup.Should().NotBeNull();

            var updateItemGroup = new ItemGroupModel
            {
                name = "Updated Group",
                description = "Updated Description",
                itemtype_id = 4
            };

            // Act
            var result = await _controller.Update(itemGroup.id, updateItemGroup) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(200);
            result?.Value.Should().Be($"ItemGroup {itemGroup.id} has been updated!");

            var updatedItemGroup = await DbContext.ItemGroups.FirstOrDefaultAsync(ig => ig.id == itemGroup.id);
            updatedItemGroup.Should().NotBeNull();
            updatedItemGroup?.name.Should().Be("Updated Group");
            updatedItemGroup?.description.Should().Be("Updated Description");
            updatedItemGroup?.itemtype_id.Should().Be(4);
        }

        [Fact]
        public async Task Update_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var itemGroup = await DbContext.ItemGroups.FirstOrDefaultAsync(ig => ig.name == "Group1");
            itemGroup.Should().NotBeNull();

            var updateItemGroup = new ItemGroupModel
            {
                name = "Updated Group",
                description = "Updated Description",
                itemtype_id = 4
            };

            // Act
            var result = await _controller.Update(itemGroup.id, updateItemGroup);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Delete_RemovesItemGroup_WhenAuthorized()
        {
            // Arrange
            var itemGroup = await DbContext.ItemGroups.FirstOrDefaultAsync(ig => ig.name == "Group1");
            itemGroup.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(itemGroup.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(200);
            result?.Value.Should().Be("ItemGroup has been deleted.");

            var deletedItemGroup = await DbContext.ItemGroups.FirstOrDefaultAsync(ig => ig.id == itemGroup.id);
            deletedItemGroup.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var itemGroup = await DbContext.ItemGroups.FirstOrDefaultAsync(ig => ig.name == "Group1");
            itemGroup.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(itemGroup.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }
    }
}
