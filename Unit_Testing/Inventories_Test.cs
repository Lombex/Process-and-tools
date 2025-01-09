using Xunit;
using Moq;
using CSharpAPI.Models;
using CSharpAPI.Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class InventoriesServiceUnitTests
    {
        [Fact]
        public async Task GetAllInventories_ReturnsListOfInventories()
        {
            // Arrange
            var mockInventories = new List<InventorieModel>
            {
                new InventorieModel { id = 1, item_id = "Item001", total_on_hand = 100 },
                new InventorieModel { id = 2, item_id = "Item002", total_on_hand = 200 }
            };

            var mockService = new Mock<IInventoriesService>();
            mockService
                .Setup(service => service.GetAllInventories())
                .ReturnsAsync(mockInventories);

            // Act
            var result = await mockService.Object.GetAllInventories();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetInventoryById_ValidId_ReturnsInventory()
        {
            // Arrange
            var mockInventory = new InventorieModel { id = 1, item_id = "Item001", total_on_hand = 100 };

            var mockService = new Mock<IInventoriesService>();
            mockService
                .Setup(service => service.GetInventoryById(1))
                .ReturnsAsync(mockInventory);

            // Act
            var result = await mockService.Object.GetInventoryById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
        }

        [Fact]
        public async Task AddInventory_ValidInventory_CallsAddMethod()
        {
            // Arrange
            var newInventory = new InventorieModel { item_id = "Item003", total_on_hand = 300 };

            var mockService = new Mock<IInventoriesService>();
            mockService
                .Setup(service => service.AddInventory(It.IsAny<InventorieModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockService.Object.AddInventory(newInventory);

            // Assert
            mockService.Verify(service => service.AddInventory(It.Is<InventorieModel>(i => i.item_id == "Item003")), Times.Once);
        }

        [Fact]
        public async Task UpdateInventory_ValidInventory_ReturnsTrue()
        {
            // Arrange
            var updatedInventory = new InventorieModel { item_id = "Item001", total_on_hand = 150 };

            var mockService = new Mock<IInventoriesService>();
            mockService
                .Setup(service => service.UpdateInventory(1, It.IsAny<InventorieModel>()))
                .ReturnsAsync(true);

            // Act
            var result = await mockService.Object.UpdateInventory(1, updatedInventory);

            // Assert
            Assert.True(result);
            mockService.Verify(service => service.UpdateInventory(1, It.Is<InventorieModel>(i => i.total_on_hand == 150)), Times.Once);
        }

        [Fact]
        public async Task UpdateInventory_InvalidInventory_ReturnsFalse()
        {
            // Arrange
            var updatedInventory = new InventorieModel { item_id = "Item004", total_on_hand = 50 };

            var mockService = new Mock<IInventoriesService>();
            mockService
                .Setup(service => service.UpdateInventory(99, It.IsAny<InventorieModel>()))
                .ReturnsAsync(false);

            // Act
            var result = await mockService.Object.UpdateInventory(99, updatedInventory);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteInventory_ValidId_ReturnsTrue()
        {
            // Arrange
            var mockService = new Mock<IInventoriesService>();
            mockService
                .Setup(service => service.DeleteInventory(1))
                .ReturnsAsync(true);

            // Act
            var result = await mockService.Object.DeleteInventory(1);

            // Assert
            Assert.True(result);
            mockService.Verify(service => service.DeleteInventory(1), Times.Once);
        }

        [Fact]
        public async Task DeleteInventory_InvalidId_ReturnsFalse()
        {
            // Arrange
            var mockService = new Mock<IInventoriesService>();
            mockService
                .Setup(service => service.DeleteInventory(99))
                .ReturnsAsync(false);

            // Act
            var result = await mockService.Object.DeleteInventory(99);

            // Assert
            Assert.False(result);
        }
    }
}
