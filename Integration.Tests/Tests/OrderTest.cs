using CSharpAPI.Controllers;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Data;
using CSharpAPI.Services.Auth;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CSharpAPI.Services;
using CSharpAPI.Services.V2;

namespace Integration.Tests.Tests
{
    public class OrderTest : IntegrationTestBase
    {
        private readonly OrdersController _controller;
        private readonly IOrderService _service;
        private readonly IAuthService _authService;
        private readonly IInventoryLocationService _inventoryLocationService;
        private readonly HistoryService _historyService;

        public OrderTest()
        {
            _historyService = new HistoryService(DbContext);
            _inventoryLocationService = new InventoryLocationService(DbContext);
            _service = new OrderService(DbContext, _historyService, _inventoryLocationService);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new OrdersController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.Order.RemoveRange(DbContext.Order);
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var orders = new List<OrderModel>
            {
                new OrderModel
                {
                    source_id = 33,
                    order_date = "2019-04-03T11:33:15Z",
                    request_date = "2019-04-07T11:33:15Z",
                    reference = "ORD00001",
                    reference_extra = "Test reference extra",
                    order_status = "Pending",
                    notes = "Test notes",
                    shipping_notes = "Test shipping notes",
                    picking_notes = "Test picking notes",
                    warehouse_id = 18,
                    ship_to = 1,
                    bill_to = 1,
                    shipment_id = 1,
                    total_amount = 9905.13f,
                    total_discount = 150.77f,
                    total_tax = 372.72f,
                    total_surcharge = 77.88886f,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow,
                    items = new List<Items>
                    {
                        new Items { item_id = "ITEM001", amount = 100 }
                    }
                },
                new OrderModel
                {
                    source_id = 34,
                    order_date = "2019-04-04T11:33:15Z",
                    reference = "ORD00002",
                    order_status = "Processing",
                    warehouse_id = 18,
                    ship_to = 1,
                    bill_to = 1,
                    total_amount = 5000f,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow,
                    items = new List<Items>
                    {
                        new Items { item_id = "ITEM002", amount = 50 }
                    }
                }
            };

            // Seed client data for ship_to and bill_to
            var client = new ClientModel
            {
                id = 1,
                name = "Test Client"
            };
            await DbContext.ClientModels.AddAsync(client);

            await DbContext.Order.AddRangeAsync(orders);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllOrders_ReturnsAllOrders_WhenAuthorized()
        {
            // Act
            var result = await _controller.GetAllOrders(10) as OkObjectResult;
            
            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            
            var orders = result.Value as List<OrderModel>;
            orders.Should().NotBeNull();
            orders.Should().HaveCount(2);
            orders.Should().Contain(o => o.reference == "ORD00001");
            orders.Should().Contain(o => o.reference == "ORD00002");
        }

        [Fact]
        public async Task GetAllOrders_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            // Act
            var result = await _controller.GetAllOrders(10);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }

        [Fact]
        public async Task GetOrderById_ReturnsOrder_WhenAuthorized()
        {
            // Arrange
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "ORD00001");
            order.Should().NotBeNull();

            // Act
            var result = await _controller.GetOrdersById(order.id) as OkObjectResult;
            var returnedOrder = result?.Value as OrderModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedOrder.Should().NotBeNull();
            returnedOrder.reference.Should().Be("ORD00001");
            returnedOrder.items.Should().NotBeNull();
            returnedOrder.items.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateOrder_AddsNewOrder_WhenAuthorized()
        {
            // Arrange
            var newOrder = new OrderModel
            {
                source_id = 35,
                order_date = "2019-04-05T11:33:15Z",
                reference = "ORD00003",
                order_status = "New",
                warehouse_id = 18,
                ship_to = 1,
                bill_to = 1,
                total_amount = 3000f,
                items = new List<Items>
                {
                    new Items { item_id = "ITEM003", amount = 30 }
                }
            };

            // Act
            var result = await _controller.CreateOrder(newOrder) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdOrder = result.Value as OrderModel;
            createdOrder.Should().NotBeNull();
            createdOrder.reference.Should().Be("ORD00003");
        }

        [Fact]
        public async Task UpdateOrder_ModifiesExistingOrder_WhenAuthorized()
        {
            // Arrange
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "ORD00001");
            order.Should().NotBeNull();

            var updateOrder = new OrderModel
            {
                reference = "ORD00001-UPD",
                order_status = "Updated",
                notes = "Updated notes",
                items = new List<Items>
                {
                    new Items { item_id = "ITEM001", amount = 150 }
                }
            };

            // Act
            var result = await _controller.UpdateOrders(order.id, updateOrder) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);

            var updatedOrder = await DbContext.Order.FirstOrDefaultAsync(o => o.id == order.id);
            updatedOrder.Should().NotBeNull();
            updatedOrder.reference.Should().Be("ORD00001-UPD");
            updatedOrder.order_status.Should().Be("Updated");
            updatedOrder.notes.Should().Be("Updated notes");
        }

        [Fact]
        public async Task DeleteOrder_RemovesOrder_WhenAuthorized()
        {
            // Arrange
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "ORD00001");
            order.Should().NotBeNull();

            // Act
            var result = await _controller.DeleteOrder(order.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);

            var deletedOrder = await DbContext.Order.FirstOrDefaultAsync(o => o.id == order.id);
            deletedOrder.Should().BeNull();
        }

        [Fact]
        public async Task DeleteOrder_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "ORD00001");
            order.Should().NotBeNull();

            // Act
            var result = await _controller.DeleteOrder(order.id);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }
    }
}