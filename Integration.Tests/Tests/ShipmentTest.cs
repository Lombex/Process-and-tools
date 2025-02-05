using CSharpAPI.Controllers;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Data;
using CSharpAPI.Services.Auth;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integration.Tests.Tests
{
    public class ShipmentsControllerTest : IntegrationTestBase
    {
        private readonly ShipmentsController _controller;
        private readonly IShipmentService _service;
        private readonly IAuthService _authService;

        public ShipmentsControllerTest()
        {
            _service = new ShipmentService(DbContext, HistoryService);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new ShipmentsController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.Shipment.RemoveRange(DbContext.Shipment);
            DbContext.Order.RemoveRange(DbContext.Order);
            DbContext.OrderShipments.RemoveRange(DbContext.OrderShipments);
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            // Seed test order data
            var orders = new List<OrderModel>
            {
                new OrderModel
                {
                    reference = "ORD001",
                    order_status = "Pending",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new OrderModel
                {
                    reference = "ORD002",
                    order_status = "Pending",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.Order.AddRangeAsync(orders);
            await DbContext.SaveChangesAsync();

            var order1 = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "ORD001");
            var order2 = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "ORD002");

            // Seed test shipment data
            var shipments = new List<ShipmentModel>
            {
                new ShipmentModel
                {
                    carrier_code = "CARRIER001",
                    shipment_status = "Pending",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ShipmentModel
                {
                    carrier_code = "CARRIER002",
                    shipment_status = "Completed",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.Shipment.AddRangeAsync(shipments);
            await DbContext.SaveChangesAsync();

            var shipment1 = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER001");
            var shipment2 = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER002");

            // Add shipment-order mapping
            var mappings = new List<OrderShipmentMapping>
            {
                new OrderShipmentMapping
                {
                    OrderId = order1.id,
                    ShipmentId = shipment1.id,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await DbContext.OrderShipments.AddRangeAsync(mappings);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_ReturnsAllShipments_WhenAuthorized()
        {
            // Act
            var actionResult = await _controller.GetAll(0);
            
            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            
            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var response = okResult.Value as object;
            response.Should().NotBeNull();
            
            var responseType = response.GetType();
            var pageProperty = responseType.GetProperty("Page").GetValue(response) as int?;
            var pageSizeProperty = responseType.GetProperty("PageSize").GetValue(response) as int?;
            var totalItemsProperty = responseType.GetProperty("TotalItems").GetValue(response) as int?;
            var totalPagesProperty = responseType.GetProperty("TotalPages").GetValue(response) as int?;
            var shipmentsProperty = responseType.GetProperty("Shipments").GetValue(response) as IEnumerable<object>;

            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(2);
            totalPagesProperty.Should().Be(1);

            shipmentsProperty.Should().NotBeNull();
            var shipments = shipmentsProperty.ToList();
            shipments.Should().HaveCount(2);

            // Check specific shipments
            var firstShipment = shipments.First();
            var firstShipmentType = firstShipment.GetType();
            (firstShipmentType.GetProperty("Carrier_code").GetValue(firstShipment) as string).Should().Be("CARRIER001");
            
            var secondShipment = shipments.Last();
            var secondShipmentType = secondShipment.GetType();
            (secondShipmentType.GetProperty("Carrier_code").GetValue(secondShipment) as string).Should().Be("CARRIER002");
        }

        [Fact]
        public async Task GetAll_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            // Act
            var result = await _controller.GetAll(0);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetById_ReturnsShipment_WhenAuthorized()
        {
            // Arrange
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER001");
            shipment.Should().NotBeNull();

            // Act
            var result = await _controller.GetById(shipment.id) as OkObjectResult;
            var returnedShipment = result?.Value as ShipmentModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedShipment.Should().NotBeNull();
            returnedShipment.carrier_code.Should().Be("CARRIER001");
            returnedShipment.shipment_status.Should().Be("Pending");
        }

        [Fact]
        public async Task GetById_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER001");
            shipment.Should().NotBeNull();

            // Act
            var result = await _controller.GetById(shipment.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        // [Fact]
        // public async Task Create_AddsNewShipment_WhenAuthorized()
        // {
        //     // Arrange
        //     var newShipment = new ShipmentModel
        //     {
        //         carrier_code = "CARRIER003",
        //         shipment_status = "Pending",
        //         created_at = DateTime.UtcNow,
        //         updated_at = DateTime.UtcNow
        //     };

        //     // Act
        //     var result = await _controller.Create(newShipment) as CreatedAtActionResult;

        //     // Assert
        //     result.Should().NotBeNull();
        //     result.StatusCode.Should().Be(201);
        //     var createdShipment = result.Value as ShipmentModel;
        //     createdShipment.Should().NotBeNull();
        //     createdShipment.carrier_code.Should().Be("CARRIER003");
        //     createdShipment.shipment_status.Should().Be("Pending");
        // }

        [Fact]
        public async Task Create_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            var newShipment = new ShipmentModel
            {
                carrier_code = "CARRIER003",
                shipment_status = "Pending"
            };

            // Act
            var result = await _controller.Create(newShipment);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Update_UpdatesExistingShipment_WhenAuthorized()
        {
            // Arrange
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER001");
            shipment.Should().NotBeNull();

            var updateShipment = new ShipmentModel
            {
                carrier_code = "CARRIER001-UPD",
                shipment_status = "Completed"
            };

            // Act
            var result = await _controller.Update(shipment.id, updateShipment) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Shipment has been updated!");

            var updatedShipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.id == shipment.id);
            updatedShipment.Should().NotBeNull();
            updatedShipment.carrier_code.Should().Be("CARRIER001-UPD");
            updatedShipment.shipment_status.Should().Be("Completed");
        }

        [Fact]
        public async Task Update_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER001");
            shipment.Should().NotBeNull();

            var updateShipment = new ShipmentModel
            {
                carrier_code = "CARRIER001-UPD",
                shipment_status = "Completed"
            };

            // Act
            var result = await _controller.Update(shipment.id, updateShipment);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Delete_RemovesShipment_WhenAuthorized()
        {
            // Arrange
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER001");
            shipment.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(shipment.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Shipment has been deleted!");

            var deletedShipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.id == shipment.id);
            deletedShipment.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER001");
            shipment.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(shipment.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetShipmentOrders_ReturnsOrders_WhenAuthorized()
        {
            // Arrange
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER001");
            shipment.Should().NotBeNull();

            // Act
            var result = await _controller.GetShipmentOrders(shipment.id) as OkObjectResult;
            var orders = result?.Value as List<OrderModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            orders.Should().NotBeNull();
            orders.Should().HaveCount(1);
            orders.Should().Contain(o => o.reference == "ORD001");
        }

        [Fact]
        public async Task GetShipmentOrders_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER001");
            shipment.Should().NotBeNull();

            // Act
            var result = await _controller.GetShipmentOrders(shipment.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task AddOrderToShipment_AddsOrder_WhenAuthorized()
        {
            // Arrange
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER002");
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "ORD002");
            shipment.Should().NotBeNull();
            order.Should().NotBeNull();

            // Act
            var result = await _controller.AddOrderToShipment(shipment.id, order.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Order added to shipment successfully");

            var orders = await _service.GetOrdersByShipmentId(shipment.id);
            orders.Should().Contain(o => o.reference == "ORD002");
        }

        [Fact]
        public async Task AddOrderToShipment_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER002");
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "ORD002");
            shipment.Should().NotBeNull();
            order.Should().NotBeNull();

            // Act
            var result = await _controller.AddOrderToShipment(shipment.id, order.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task RemoveOrderFromShipment_RemovesOrder_WhenAuthorized()
        {
            // Arrange
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER001");
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "ORD001");
            shipment.Should().NotBeNull();
            order.Should().NotBeNull();

            // Act
            var result = await _controller.RemoveOrderFromShipment(shipment.id, order.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Order removed from shipment successfully");

            var orders = await _service.GetOrdersByShipmentId(shipment.id);
            orders.Should().NotContain(o => o.reference == "ORD001");
        }

        [Fact]
        public async Task RemoveOrderFromShipment_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var shipment = await DbContext.Shipment.FirstOrDefaultAsync(s => s.carrier_code == "CARRIER001");
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "ORD001");
            shipment.Should().NotBeNull();
            order.Should().NotBeNull();

            // Act
            var result = await _controller.RemoveOrderFromShipment(shipment.id, order.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }
    }
}
