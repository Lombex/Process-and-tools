using CSharpAPI.Service;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Integration.Tests.Infrastructure;

namespace Integration.Tests.Tests
{
    public class ItemGroupTest : IntegrationTestBase
    {
        private readonly ItemGroupService _service;

        public ItemGroupTest()
        {
            _service = new ItemGroupService(DbContext);
            SeedTestData();
        }

        private async void SeedTestData()
        {
            var itemGroups = new List<ItemGroupModel>
            {
                new ItemGroupModel 
                { 
                    id = 1, 
                    name = "Group1", 
                    description = "Test Group 1",
                    itemtype_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemGroupModel 
                { 
                    id = 2, 
                    name = "Group2",
                    description = "Test Group 2",
                    itemtype_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };
            await DbContext.ItemGroups.AddRangeAsync(itemGroups);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_ReturnsAllItemGroups()
        {
            // Act
            var result = await _service.GetAll();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(i => i.name == "Group1");
            result.Should().Contain(i => i.name == "Group2");
        }

        [Fact]
        public async Task GetById_ReturnsItemGroup()
        {
            // Act
            var result = await _service.GetById(1);

            // Assert
            result.Should().NotBeNull();
            result.name.Should().Be("Group1");
            result.description.Should().Be("Test Group 1");
        }

        [Fact]
        public async Task GetById_ThrowsException_WhenNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetById(999));
        }

        [Fact]
        public async Task Add_AddsNewItemGroup()
        {
            // Arrange
            var itemGroup = new ItemGroupModel 
            { 
                name = "Group3",
                description = "Test Group 3",
                itemtype_id = 1
            };

            // Act
            await _service.Add(itemGroup);

            // Assert
            var addedItemGroup = await DbContext.ItemGroups.FirstOrDefaultAsync(i => i.name == "Group3");
            addedItemGroup.Should().NotBeNull();
            addedItemGroup.name.Should().Be("Group3");
            addedItemGroup.description.Should().Be("Test Group 3");
        }

        [Fact]
        public async Task Update_UpdatesExistingItemGroup()
        {
            // Arrange
            var updateModel = new ItemGroupModel
            {
                name = "Updated Group",
                description = "Updated Description",
                itemtype_id = 1
            };

            // Act
            var result = await _service.Update(1, updateModel);

            // Assert
            result.Should().BeTrue();
            var updated = await _service.GetById(1);
            updated.name.Should().Be("Updated Group");
            updated.description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task Update_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            var updateModel = new ItemGroupModel
            {
                name = "Updated Group",
                description = "Updated Description",
                itemtype_id = 1
            };

            // Act
            var result = await _service.Update(999, updateModel);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_RemovesItemGroup()
        {
            // Act
            var result = await _service.Delete(1);

            // Assert
            result.Should().BeTrue();
            var itemGroups = await _service.GetAll();
            itemGroups.Should().NotContain(i => i.id == 1);
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotFound()
        {
            // Act
            var result = await _service.Delete(999);

            // Assert
            result.Should().BeFalse();
        }
    }
}