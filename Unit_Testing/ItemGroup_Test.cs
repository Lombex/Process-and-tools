using Xunit;
using Moq;
using CSharpAPI.Models;
using CSharpAPI.Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class ItemGroupServiceUnitTests
    {
        [Fact]
        public async Task GetAll_ReturnsListOfItemGroups()
        {
            // Arrange
            var mockItemGroups = new List<ItemGroupModel>
            {
                new ItemGroupModel { id = 1, name = "Group A", description = "Description A", itemtype_id = 1 },
                new ItemGroupModel { id = 2, name = "Group B", description = "Description B", itemtype_id = 2 }
            };

            var mockService = new Mock<IItemGroupService>();
            mockService
                .Setup(service => service.GetAll())
                .ReturnsAsync(mockItemGroups);

            // Act
            var result = await mockService.Object.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsItemGroup()
        {
            // Arrange
            var mockItemGroup = new ItemGroupModel { id = 1, name = "Group A", description = "Description A", itemtype_id = 1 };

            var mockService = new Mock<IItemGroupService>();
            mockService
                .Setup(service => service.GetById(1))
                .ReturnsAsync(mockItemGroup);

            // Act
            var result = await mockService.Object.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Group A", result.name);
        }

        [Fact]
        public async Task Add_ValidItemGroup_CallsAddMethod()
        {
            // Arrange
            var newItemGroup = new ItemGroupModel { name = "Group C", description = "Description C", itemtype_id = 3 };

            var mockService = new Mock<IItemGroupService>();
            mockService
                .Setup(service => service.Add(It.IsAny<ItemGroupModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockService.Object.Add(newItemGroup);

            // Assert
            mockService.Verify(service => service.Add(It.Is<ItemGroupModel>(g => g.name == "Group C")), Times.Once);
        }

        [Fact]
        public async Task Update_ValidItemGroup_CallsUpdateMethod()
        {
            // Arrange
            var updatedItemGroup = new ItemGroupModel { name = "Updated Group A", description = "Updated Description A", itemtype_id = 1 };

            var mockService = new Mock<IItemGroupService>();
            mockService
                .Setup(service => service.Update(1, It.IsAny<ItemGroupModel>()))
                .ReturnsAsync(true);

            // Act
            var result = await mockService.Object.Update(1, updatedItemGroup);

            // Assert
            Assert.True(result);
            mockService.Verify(service => service.Update(1, It.Is<ItemGroupModel>(g => g.name == "Updated Group A")), Times.Once);
        }

        [Fact]
        public async Task Delete_ValidItemGroup_CallsDeleteMethod()
        {
            // Arrange
            var mockService = new Mock<IItemGroupService>();
            mockService
                .Setup(service => service.Delete(1))
                .ReturnsAsync(true);

            // Act
            var result = await mockService.Object.Delete(1);

            // Assert
            Assert.True(result);
            mockService.Verify(service => service.Delete(1), Times.Once);
        }
    }
}
