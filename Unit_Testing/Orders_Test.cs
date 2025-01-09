using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class OrderServiceUnitTests
    {
        [Fact]
        public async Task GetAllOrders_ReturnsListOfOrders()
        {
            // Arrange
            var mockOrders = new List<OrderModel>
            {
                new OrderModel
                {
                    id = 1,
                    reference = "ORD00001",
                    order_status = "Delivered",
                    total_amount = 500.0f
                },
                new OrderModel
                {
                    id = 2,
                    reference = "ORD00002",
                    order_status = "Pending",
                    total_amount = 750.0f
                }
            };

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService
                .Setup(service => service.GetAllOrders())
                .ReturnsAsync(mockOrders);

            // Act
            var result = await mockOrderService.Object.GetAllOrders();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("ORD00001", result[0].reference);
            Assert.Equal("ORD00002", result[1].reference);
        }

        [Fact]
        public async Task GetOrderById_ValidId_ReturnsOrder()
        {
            // Arrange
            var order = new OrderModel
            {
                id = 1,
                reference = "ORD00001",
                order_status = "Delivered",
                total_amount = 500.0f
            };

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService
                .Setup(service => service.GetOrderById(1))
                .ReturnsAsync(order);

            // Act
            var result = await mockOrderService.Object.GetOrderById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("ORD00001", result.reference);
            Assert.Equal("Delivered", result.order_status);
        }

        [Fact]
        public async Task CreateOrder_ValidOrder_CallsCreateMethod()
        {
            // Arrange
            var newOrder = new OrderModel
            {
                reference = "ORD00003",
                order_status = "Pending",
                total_amount = 1200.0f
            };

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService
                .Setup(service => service.CreateOrder(It.IsAny<OrderModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockOrderService.Object.CreateOrder(newOrder);

            // Assert
            mockOrderService.Verify(service => service.CreateOrder(It.Is<OrderModel>(o => o.reference == "ORD00003" && o.total_amount == 1200.0f)), Times.Once);
        }

        [Fact]
        public async Task UpdateOrders_ValidOrder_CallsUpdateMethod()
        {
            // Arrange
            var updatedOrder = new OrderModel
            {
                id = 1,
                reference = "ORD00001",
                order_status = "Shipped",
                total_amount = 1300.0f
            };

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService
                .Setup(service => service.UpdateOrders(It.IsAny<int>(), It.IsAny<OrderModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockOrderService.Object.UpdateOrders(1, updatedOrder);

            // Assert
            mockOrderService.Verify(service => service.UpdateOrders(1, It.Is<OrderModel>(o => o.order_status == "Shipped" && o.total_amount == 1300.0f)), Times.Once);
        }

        [Fact]
        public async Task DeleteOrder_ValidId_CallsDeleteMethod()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            mockOrderService
                .Setup(service => service.DeleteOrder(1))
                .Returns(Task.CompletedTask);

            // Act
            await mockOrderService.Object.DeleteOrder(1);

            // Assert
            mockOrderService.Verify(service => service.DeleteOrder(1), Times.Once);
        }
    }
}
