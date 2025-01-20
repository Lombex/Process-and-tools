using CSharpAPI.Controller;
using CSharpAPI.Models;
using CSharpAPI.Service;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Integration.Tests.Tests
{
    public class OrdersControllerTest : IntegrationTestBase
    {
        private readonly OrdersControllers _controller;
        private readonly IOrderService _service;

        public OrdersControllerTest()
        {
            _service = new OrderService(DbContext);
            _controller = new OrdersControllers(_service);

            SeedTestData();
        }

        private async void SeedTestData()
        {
            var orders = new List<OrderModel>
            {
                new OrderModel
                {
                    id = 1,
                    source_id = 101,
                    order_date = "2025-01-01",
                    request_date = "2025-01-05",
                    reference = "REF001",
                    order_status = "Pending",
                    warehouse_id = 1,
                    ship_to = 1,
                    bill_to = 1,
                    total_amount = 100.50f,
                    total_discount = 10.00f,
                    total_tax = 5.50f,
                    total_surcharge = 2.00f,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new OrderModel
                {
                    id = 2,
                    source_id = 102,
                    order_date = "2025-01-02",
                    request_date = "2025-01-06",
                    reference = "REF002",
                    order_status = "Completed",
                    warehouse_id = 2,
                    ship_to = 2,
                    bill_to = 2,
                    total_amount = 200.75f,
                    total_discount = 15.00f,
                    total_tax = 10.50f,
                    total_surcharge = 3.00f,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.Order.AddRangeAsync(orders);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_ReturnsAllOrders()
        {
            var result = await _controller.GetAllOrders() as OkObjectResult;
            var orders = result?.Value as List<OrderModel>;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            orders.Should().HaveCount(2);
            orders.Should().Contain(o => o.reference == "REF001");
            orders.Should().Contain(o => o.reference == "REF002");
        }

        [Fact]
        public async Task GetById_ReturnsOrder()
        {
            var result = await _controller.GetOrdersById(1) as OkObjectResult;
            var order = result?.Value as OrderModel;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            order.Should().NotBeNull();
            order.reference.Should().Be("REF001");
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_ForInvalidId()
        {
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.GetOrdersById(999);
            });

            exception.Message.Should().Be("Order not found!");
        }

        [Fact]
        public async Task Create_AddsNewOrder()
        {
            var newOrder = new OrderModel
            {
                source_id = 103,
                order_date = "2025-01-03",
                request_date = "2025-01-07",
                reference = "REF003",
                order_status = "Pending",
                warehouse_id = 3,
                ship_to = 3,
                bill_to = 3,
                total_amount = 300.25f,
                total_discount = 20.00f,
                total_tax = 15.00f,
                total_surcharge = 5.00f
            };

            var result = await _controller.CreateOrder(newOrder) as CreatedAtActionResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdOrder = result.Value as OrderModel;
            createdOrder.Should().NotBeNull();
            createdOrder.reference.Should().Be("REF003");
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelIsNull()
        {
            var result = await _controller.CreateOrder(null) as BadRequestObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
            result.Value.Should().Be("Request is empty!");
        }

        [Fact]
        public async Task Update_UpdatesExistingOrder()
        {
            var updatedOrder = new OrderModel
            {
                source_id = 101,
                order_date = "2025-01-04",
                request_date = "2025-01-08",
                reference = "REF001-UPD",
                order_status = "Shipped",
                warehouse_id = 1,
                ship_to = 1,
                bill_to = 1,
                total_amount = 150.75f,
                total_discount = 15.00f,
                total_tax = 7.50f,
                total_surcharge = 2.50f
            };

            var result = await _controller.UpdateOrders(1, updatedOrder) as OkObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Transfer 1 has been updated!");
        }

        [Fact]
        public async Task Update_ReturnsNotFound_ForInvalidId()
        {
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                var updatedOrder = new OrderModel
                {
                    source_id = 999,
                    reference = "INVALID"
                };
                await _controller.UpdateOrders(999, updatedOrder);
            });

            exception.Message.Should().Be("Order not found!");
        }

        [Fact]
        public async Task Delete_RemovesOrder()
        {
            var result = await _controller.DeleteOrder(1) as OkObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Order has been deleted!");

            var orders = await _service.GetAllOrders();
            orders.Should().NotContain(o => o.id == 1);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_ForInvalidId()
        {
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.DeleteOrder(999);
            });

            exception.Message.Should().Be("Order not found!");
        }
    }
}
