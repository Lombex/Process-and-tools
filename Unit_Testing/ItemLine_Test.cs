using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class ItemLineServiceUnitTests
    {
        [Fact]
        public async Task GetAllItemLines_ReturnsListOfItemLines()
        {
            // Arrange
            var mockItemLines = new List<ItemLineModel>
            {
                new ItemLineModel { id = 1, name = "Item Line A", description = "Description for Item Line A" },
                new ItemLineModel { id = 2, name = "Item Line B", description = "Description for Item Line B" }
            };

            var mockItemLineService = new Mock<IItemLineService>();
            mockItemLineService
                .Setup(service => service.GetAllItemLines())
                .ReturnsAsync(mockItemLines);

            // Act
            var result = await mockItemLineService.Object.GetAllItemLines();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Item Line A", result.First().name);
            Assert.Equal("Item Line B", result.Last().name);
        }

        [Fact]
        public async Task GetItemLineById_ValidId_ReturnsItemLine()
        {
            // Arrange
            var mockItemLine = new ItemLineModel { id = 1, name = "Item Line A", description = "Description for Item Line A" };

            var mockItemLineService = new Mock<IItemLineService>();
            mockItemLineService
                .Setup(service => service.GetItemLineById(1))
                .ReturnsAsync(mockItemLine);

            // Act
            var result = await mockItemLineService.Object.GetItemLineById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Item Line A", result.name);
        }

        [Fact]
        public async Task CreateItemLine_ValidItemLine_CallsCreateMethod()
        {
            // Arrange
            var newItemLine = new ItemLineModel { name = "Item Line C", description = "Description for Item Line C" };

            var mockItemLineService = new Mock<IItemLineService>();
            mockItemLineService
                .Setup(service => service.CreateItemLine(It.IsAny<ItemLineModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockItemLineService.Object.CreateItemLine(newItemLine);

            // Assert
            mockItemLineService.Verify(service => service.CreateItemLine(It.Is<ItemLineModel>(i => i.name == "Item Line C" && i.description == "Description for Item Line C")), Times.Once);
        }

        [Fact]
        public async Task UpdateItemLine_ValidItemLine_CallsUpdateMethod()
        {
            // Arrange
            var updatedItemLine = new ItemLineModel { name = "Updated Item Line A", description = "Updated description for Item Line A" };

            var mockItemLineService = new Mock<IItemLineService>();
            mockItemLineService
                .Setup(service => service.UpdateItemLine(It.IsAny<int>(), It.IsAny<ItemLineModel>()))
                .ReturnsAsync(true); // Simuleer succesvolle update

            // Act
            var result = await mockItemLineService.Object.UpdateItemLine(1, updatedItemLine);

            // Assert
            Assert.True(result);
            mockItemLineService.Verify(service => service.UpdateItemLine(1, It.Is<ItemLineModel>(i => i.name == "Updated Item Line A")), Times.Once);
        }

        [Fact]
        public async Task DeleteItemLine_ValidId_CallsDeleteMethod()
        {
            // Arrange
            var mockItemLineService = new Mock<IItemLineService>();
            mockItemLineService
                .Setup(service => service.DeleteItemLine(1))
                .ReturnsAsync(true); // Simuleer succesvolle verwijdering

            // Act
            var result = await mockItemLineService.Object.DeleteItemLine(1);

            // Assert
            Assert.True(result);
            mockItemLineService.Verify(service => service.DeleteItemLine(1), Times.Once);
        }
    }
}
