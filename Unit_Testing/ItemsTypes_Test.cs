using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class ItemTypeServiceUnitTests
    {
        [Fact]
        public async Task GetAll_ReturnsListOfItemTypes()
        {
            // Arrange
            var mockItemTypes = new List<ItemTypeModel>
            {
                new ItemTypeModel { id = 1, name = "Type A", description = "Description A" },
                new ItemTypeModel { id = 2, name = "Type B", description = "Description B" }
            };

            var mockItemTypeService = new Mock<IItemTypeService>();
            mockItemTypeService
                .Setup(service => service.GetAll())
                .ReturnsAsync(mockItemTypes);

            // Act
            var result = await mockItemTypeService.Object.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Type A", result[0].name);
            Assert.Equal("Type B", result[1].name);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsItemType()
        {
            // Arrange
            var mockItemType = new ItemTypeModel { id = 1, name = "Type A", description = "Description A" };

            var mockItemTypeService = new Mock<IItemTypeService>();
            mockItemTypeService
                .Setup(service => service.GetById(1))
                .ReturnsAsync(mockItemType);

            // Act
            var result = await mockItemTypeService.Object.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Type A", result.name);
        }

        [Fact]
        public async Task Add_ValidItemType_CallsAddMethod()
        {
            // Arrange
            var newItemType = new ItemTypeModel { name = "Type A", description = "Description A" };

            var mockItemTypeService = new Mock<IItemTypeService>();
            mockItemTypeService
                .Setup(service => service.Add(It.IsAny<ItemTypeModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockItemTypeService.Object.Add(newItemType);

            // Assert
            mockItemTypeService.Verify(service => service.Add(It.Is<ItemTypeModel>(i => i.name == "Type A" && i.description == "Description A")), Times.Once);
        }

        [Fact]
        public async Task Update_ValidItemType_CallsUpdateMethod()
        {
            // Arrange
            var updatedItemType = new ItemTypeModel { name = "Updated Type A", description = "Updated Description A" };

            var mockItemTypeService = new Mock<IItemTypeService>();
            mockItemTypeService
                .Setup(service => service.Update(It.IsAny<int>(), It.IsAny<ItemTypeModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockItemTypeService.Object.Update(1, updatedItemType);

            // Assert
            mockItemTypeService.Verify(service => service.Update(1, It.Is<ItemTypeModel>(i => i.name == "Updated Type A")), Times.Once);
        }

        [Fact]
        public async Task Delete_ValidId_CallsDeleteMethod()
        {
            // Arrange
            var mockItemTypeService = new Mock<IItemTypeService>();
            mockItemTypeService
                .Setup(service => service.Delete(1))
                .Returns(Task.CompletedTask);

            // Act
            await mockItemTypeService.Object.Delete(1);

            // Assert
            mockItemTypeService.Verify(service => service.Delete(1), Times.Once);
        }
    }
}
