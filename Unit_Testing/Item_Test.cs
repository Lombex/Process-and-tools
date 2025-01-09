using Xunit;
using Moq;
using CSharpAPI.Models;
using CSharpAPI.Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class ItemsServiceUnitTests
    {
        [Fact]
        public async Task GetAllItems_ReturnsListOfItems()
        {
            // Arrange
            var mockItems = new List<ItemModel>
            {
                new ItemModel { uid = "1", code = "A001", description = "Item A" },
                new ItemModel { uid = "2", code = "B001", description = "Item B" }
            };

            var mockService = new Mock<IItemsService>();
            mockService
                .Setup(service => service.GetAllItems())
                .ReturnsAsync(mockItems);

            // Act
            var result = await mockService.Object.GetAllItems();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetItemById_ValidUid_ReturnsItem()
        {
            // Arrange
            var mockItem = new ItemModel { uid = "1", code = "A001", description = "Item A" };

            var mockService = new Mock<IItemsService>();
            mockService
                .Setup(service => service.GetItemById("1"))
                .ReturnsAsync(mockItem);

            // Act
            var result = await mockService.Object.GetItemById("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.uid);
        }

        [Fact]
        public async Task CreateItem_ValidItem_CallsCreateMethod()
        {
            // Arrange
            var newItem = new ItemModel { code = "C001", description = "Item C" };

            var mockService = new Mock<IItemsService>();
            mockService
                .Setup(service => service.CreateItem(It.IsAny<ItemModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockService.Object.CreateItem(newItem);

            // Assert
            mockService.Verify(service => service.CreateItem(It.Is<ItemModel>(i => i.code == "C001")), Times.Once);
        }

        [Fact]
        public async Task UpdateItem_ValidItem_CallsUpdateMethod()
        {
            // Arrange
            var updatedItem = new ItemModel { code = "Updated Code", description = "Updated Description" };

            var mockService = new Mock<IItemsService>();
            mockService
                .Setup(service => service.UpdateItem("1", It.IsAny<ItemModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockService.Object.UpdateItem("1", updatedItem);

            // Assert
            mockService.Verify(service => service.UpdateItem("1", It.Is<ItemModel>(i => i.code == "Updated Code")), Times.Once);
        }

        [Fact]
        public async Task DeleteItem_ValidUid_CallsDeleteMethod()
        {
            // Arrange
            var mockService = new Mock<IItemsService>();
            mockService
                .Setup(service => service.DeleteItem("1"))
                .Returns(Task.CompletedTask);

            // Act
            await mockService.Object.DeleteItem("1");

            // Assert
            mockService.Verify(service => service.DeleteItem("1"), Times.Once);
        }
    }
}
