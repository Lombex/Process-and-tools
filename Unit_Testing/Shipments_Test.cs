using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class ShipmentServiceUnitTests
    {
        [Fact]
        public async Task GetAll_ReturnsListOfShipments()
        {
            // Arrange
            var mockShipments = new List<ShipmentModel>
            {
                new ShipmentModel
                {
                    id = 1,
                    order_id = 1,
                    shipment_type = "Air",
                    shipment_status = "Shipped"
                },
                new ShipmentModel
                {
                    id = 2,
                    order_id = 2,
                    shipment_type = "Sea",
                    shipment_status = "Pending"
                }
            };

            var mockShipmentService = new Mock<IShipmentService>();
            mockShipmentService
                .Setup(service => service.GetAll())
                .ReturnsAsync(mockShipments);

            // Act
            var result = await mockShipmentService.Object.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Air", result[0].shipment_type);
            Assert.Equal("Sea", result[1].shipment_type);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsShipment()
        {
            // Arrange
            var shipment = new ShipmentModel
            {
                id = 1,
                order_id = 1,
                shipment_type = "Air",
                shipment_status = "Shipped"
            };

            var mockShipmentService = new Mock<IShipmentService>();
            mockShipmentService
                .Setup(service => service.GetById(1))
                .ReturnsAsync(shipment);

            // Act
            var result = await mockShipmentService.Object.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Air", result.shipment_type);
        }

        [Fact]
        public async Task Add_ValidShipment_CallsAddMethod()
        {
            // Arrange
            var newShipment = new ShipmentModel
            {
                order_id = 3,
                shipment_type = "Ground",
                shipment_status = "In Progress"
            };

            var mockShipmentService = new Mock<IShipmentService>();
            mockShipmentService
                .Setup(service => service.Add(It.IsAny<ShipmentModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockShipmentService.Object.Add(newShipment);

            // Assert
            mockShipmentService.Verify(service => service.Add(It.Is<ShipmentModel>(s => s.order_id == 3 && s.shipment_type == "Ground")), Times.Once);
        }

        [Fact]
        public async Task Update_ValidShipment_CallsUpdateMethod()
        {
            // Arrange
            var updatedShipment = new ShipmentModel
            {
                id = 1,
                order_id = 1,
                shipment_type = "Sea",
                shipment_status = "Shipped"
            };

            var mockShipmentService = new Mock<IShipmentService>();
            mockShipmentService
                .Setup(service => service.Update(It.IsAny<int>(), It.IsAny<ShipmentModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockShipmentService.Object.Update(1, updatedShipment);

            // Assert
            mockShipmentService.Verify(service => service.Update(1, It.Is<ShipmentModel>(s => s.shipment_type == "Sea")), Times.Once);
        }

        [Fact]
        public async Task Delete_ValidId_CallsDeleteMethod()
        {
            // Arrange
            var mockShipmentService = new Mock<IShipmentService>();
            mockShipmentService
                .Setup(service => service.Delete(1))
                .Returns(Task.CompletedTask);

            // Act
            await mockShipmentService.Object.Delete(1);

            // Assert
            mockShipmentService.Verify(service => service.Delete(1), Times.Once);
        }
    }
}
