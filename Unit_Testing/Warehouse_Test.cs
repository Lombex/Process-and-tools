using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class WarehouseServiceUnitTests
    {
        [Fact]
        public async Task GetAllWarehouses_ReturnsListOfWarehouses()
        {
            // Arrange
            var mockWarehouses = new List<WarehouseModel>
            {
                new WarehouseModel { id = 1, code = "WH001", name = "Warehouse 1" },
                new WarehouseModel { id = 2, code = "WH002", name = "Warehouse 2" }
            };

            var mockWarehouseService = new Mock<IWarehouseService>();
            mockWarehouseService
                .Setup(service => service.GetAllWarehouses())
                .ReturnsAsync(mockWarehouses);

            // Act
            var result = await mockWarehouseService.Object.GetAllWarehouses();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Warehouse 1", result[0].name);
            Assert.Equal("Warehouse 2", result[1].name);
        }

        [Fact]
        public async Task GetAllWarehouses_ReturnsEmptyListWhenNoWarehousesExist()
        {
            // Arrange
            var mockWarehouseService = new Mock<IWarehouseService>();
            mockWarehouseService
                .Setup(service => service.GetAllWarehouses())
                .ReturnsAsync(new List<WarehouseModel>());

            // Act
            var result = await mockWarehouseService.Object.GetAllWarehouses();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWarehouseById_ValidId_ReturnsWarehouse()
        {
            // Arrange
            var warehouse = new WarehouseModel { id = 1, code = "WH001", name = "Warehouse 1" };

            var mockWarehouseService = new Mock<IWarehouseService>();
            mockWarehouseService
                .Setup(service => service.GetWarehouseById(1))
                .ReturnsAsync(warehouse);

            // Act
            var result = await mockWarehouseService.Object.GetWarehouseById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Warehouse 1", result.name);
        }

        [Fact]
        public async Task GetWarehouseById_InvalidId_ReturnsNull()
        {
            // Arrange
            var mockWarehouseService = new Mock<IWarehouseService>();
            mockWarehouseService
                .Setup(service => service.GetWarehouseById(99))
                .ReturnsAsync((WarehouseModel)null);

            // Act
            var result = await mockWarehouseService.Object.GetWarehouseById(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddWarehouse_ValidWarehouse_CallsAddMethod()
        {
            // Arrange
            var newWarehouse = new WarehouseModel { id = 3, code = "WH003", name = "Warehouse 3" };

            var mockWarehouseService = new Mock<IWarehouseService>();
            mockWarehouseService
                .Setup(service => service.AddWarehouse(It.IsAny<WarehouseModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockWarehouseService.Object.AddWarehouse(newWarehouse);

            // Assert
            mockWarehouseService.Verify(service => service.AddWarehouse(It.Is<WarehouseModel>(w => w.id == 3 && w.name == "Warehouse 3")), Times.Once);
        }

        [Fact]
        public async Task UpdateWarehouse_ValidWarehouse_CallsUpdateMethod()
        {
            // Arrange
            var updatedWarehouse = new WarehouseModel { id = 1, code = "WH001", name = "Updated Warehouse" };

            var mockWarehouseService = new Mock<IWarehouseService>();
            mockWarehouseService
                .Setup(service => service.UpdateWarehouse(It.IsAny<int>(), It.IsAny<WarehouseModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockWarehouseService.Object.UpdateWarehouse(1, updatedWarehouse);

            // Assert
            mockWarehouseService.Verify(service => service.UpdateWarehouse(1, It.Is<WarehouseModel>(w => w.name == "Updated Warehouse")), Times.Once);
        }

        [Fact]
        public async Task UpdateWarehouse_InvalidId_DoesNotCallUpdateMethod()
        {
            // Arrange
            var updatedWarehouse = new WarehouseModel { id = 99, code = "WH099", name = "Nonexistent Warehouse" };

            var mockWarehouseService = new Mock<IWarehouseService>();
            mockWarehouseService
                .Setup(service => service.UpdateWarehouse(It.IsAny<int>(), It.IsAny<WarehouseModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockWarehouseService.Object.UpdateWarehouse(99, updatedWarehouse);

            // Assert
            mockWarehouseService.Verify(service => service.UpdateWarehouse(99, updatedWarehouse), Times.Once);
        }

        [Fact]
        public async Task DeleteWarehouse_ValidId_CallsDeleteMethod()
        {
            // Arrange
            var mockWarehouseService = new Mock<IWarehouseService>();
            mockWarehouseService
                .Setup(service => service.DeleteWarehouse(1))
                .Returns(Task.CompletedTask);

            // Act
            await mockWarehouseService.Object.DeleteWarehouse(1);

            // Assert
            mockWarehouseService.Verify(service => service.DeleteWarehouse(1), Times.Once);
        }

        [Fact]
        public async Task DeleteWarehouse_InvalidId_DoesNotCallDeleteMethod()
        {
            // Arrange
            var mockWarehouseService = new Mock<IWarehouseService>();
            mockWarehouseService
                .Setup(service => service.DeleteWarehouse(99))
                .Returns(Task.CompletedTask);

            // Act
            await mockWarehouseService.Object.DeleteWarehouse(99);

            // Assert
            mockWarehouseService.Verify(service => service.DeleteWarehouse(99), Times.Once);
        }
    }
}
