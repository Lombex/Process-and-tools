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
using CSharpAPI.Services.V2;

namespace Integration.Tests.Tests
{
    public class OrdersControllerTest : IntegrationTestBase
    {
        private readonly OrdersController _controller;
        private readonly IOrderService _service;
        private readonly IAuthService _authService;
        private readonly IInventoryLocationService _inventoryLocationService;

    public OrdersControllerTest()
    {
        _service = new OrderService(DbContext, HistoryService, _inventoryLocationService);
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
        // First seed warehouse data if needed
        var warehouse = new WarehouseModel
        {
            id = 1,
            code = "WH001",
            name = "Test Warehouse",
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        var mockClient = new ClientModel
        {
        id = 1,  // Zorg ervoor dat deze ID overeenkomt met ship_to en bill_to
        name = "Test Client",
        address = "123 Test Street",
        city = "Test City",
        zip_code = "12345",
        province = "Test Province",
        country = "Testland",
        contact = new Contact
        {
            name = "John Doe",
            phone = "+123456789",
            email = "test@example.com"
        },
        created_at = DateTime.UtcNow,
        updated_at = DateTime.UtcNow
        };
        await DbContext.ClientModels.AddAsync(mockClient);
        await DbContext.SaveChangesAsync();
    
        await DbContext.Warehouse.AddAsync(warehouse);
        await DbContext.SaveChangesAsync();
    
        // Then seed orders
        var orders = new List<OrderModel>
        {
            new OrderModel
            {
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
                updated_at = DateTime.UtcNow,
                notes = "Test Order 1",
                shipping_notes = "Test Shipping 1",
                picking_notes = "Test Picking 1"
            },
            new OrderModel
            {
                source_id = 102,
                order_date = "2025-01-02",
                request_date = "2025-01-06",
                reference = "REF002",
                order_status = "Completed",
                warehouse_id = 1,
                ship_to = 1,
                bill_to = 1,
                total_amount = 200.75f,
                total_discount = 15.00f,
                total_tax = 10.50f,
                total_surcharge = 3.00f,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                notes = "Test Order 2",
                shipping_notes = "Test Shipping 2",
                picking_notes = "Test Picking 2"
            }
        };
    
        await DbContext.Order.AddRangeAsync(orders);
        await DbContext.SaveChangesAsync();
    }


        [Fact]
        public async Task GetAllOrders_ReturnsAllOrders_WhenAuthorized()
        {
            // Act
            var actionResult = await _controller.GetAllOrders(0);

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
            var ordersProperty = responseType.GetProperty("Order").GetValue(response) as IEnumerable<object>;
            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(2);
            totalPagesProperty.Should().Be(1);
            ordersProperty.Should().NotBeNull();
            var orders = ordersProperty.ToList();
            orders.Should().HaveCount(2);
            // Check first order
            var firstOrder = orders.First();
            var firstOrderType = firstOrder.GetType();
            (firstOrderType.GetProperty("Reference").GetValue(firstOrder) as string).Should().Be("REF001");
            (firstOrderType.GetProperty("Order_status").GetValue(firstOrder) as string).Should().Be("Pending");
            // Check second order
            var secondOrder = orders.Last();
            var secondOrderType = secondOrder.GetType();
            (secondOrderType.GetProperty("Reference").GetValue(secondOrder) as string).Should().Be("REF002");
            (secondOrderType.GetProperty("Order_status").GetValue(secondOrder) as string).Should().Be("Completed");
        }

        [Fact]
        public async Task GetAllOrders_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            // Act
            var result = await _controller.GetAllOrders(0);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetOrderById_ReturnsOrder_WhenAuthorized()
        {
            // Arrange
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "REF001");
            order.Should().NotBeNull();

            // Act
            var result = await _controller.GetOrdersById(order.id) as OkObjectResult;
            var returnedOrder = result?.Value as OrderModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedOrder.Should().NotBeNull();
            returnedOrder.reference.Should().Be("REF001");
            returnedOrder.order_status.Should().Be("Pending");
            returnedOrder.total_amount.Should().Be(100.50f);
        }
        [Fact]
        public async Task GetOrderById_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "REF001");
            order.Should().NotBeNull();
            // Act
            var result = await _controller.GetOrdersById(order.id);
            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task CreateOrder_AddsNewOrder_WhenAuthorized()
        {
            
            // Arrange
            var newOrder = new OrderModel
            {
                source_id = 103,
                order_date = "2025-01-03",
                request_date = "2025-01-07",
                reference = "REF003",
                reference_extra = null, // Aangezien dit veld nieuw is, kun je het expliciet op null zetten
                order_status = "Pending",
                notes = null, // Indien nodig kun je hier een string toevoegen
                shipping_notes = null,
                picking_notes = null,
                warehouse_id = 1,
                shipment_id = null, // Nieuw optioneel veld, zet op null als het niet nodig is
                ship_to = 1,
                bill_to = 1,
                total_amount = 300.25f,
                total_discount = 20.00f,
                total_tax = 15.00f,
                total_surcharge = 5.00f,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                items = new List<Items>() // Voeg een lege lijst toe als items vereist is
            };

            // Act
            var result = await _controller.CreateOrder(newOrder) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdOrder = result.Value as OrderModel;
            createdOrder.Should().NotBeNull();
            createdOrder.reference.Should().Be("REF003");
            createdOrder.total_amount.Should().Be(300.25f);
        }
        [Fact]
        public async Task CreateOrder_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var newOrder = new OrderModel
            {
                reference = "REF003",
                order_status = "Pending"
            };

            // Act
            var result = await _controller.CreateOrder(newOrder);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task UpdateOrder_UpdatesExistingOrder_WhenAuthorized()
        {
            // Arrange
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "REF001");
            order.Should().NotBeNull();

            var updateOrder = new OrderModel
            {
                source_id = 101,
                reference = "REF001-UPD",
                order_status = "Completed",
                warehouse_id = 1,
                ship_to = 1,
                bill_to = 1,
                total_amount = 150.75f
            };

            // Act
            var result = await _controller.UpdateOrders(order.id, updateOrder) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be($"Transfer {order.id} has been updated!");

            var updatedOrder = await DbContext.Order.FirstOrDefaultAsync(o => o.id == order.id);
            updatedOrder.Should().NotBeNull();
            updatedOrder.reference.Should().Be("REF001-UPD");
            updatedOrder.order_status.Should().Be("Completed");
        }
        [Fact]
        public async Task UpdateOrder_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "REF001");
            order.Should().NotBeNull();
            var updateOrder = new OrderModel
            {
                reference = "REF001-UPD",
                order_status = "Completed"
            };
            // Act
            var result = await _controller.UpdateOrders(order.id, updateOrder);
            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        // [Fact]
        // public async Task DeleteOrder_RemovesOrder_WhenAuthorized()
        // {
        //     // Arrange
        //     var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "REF001");
        //     order.Should().NotBeNull();

        //     // Act
        //     var result = await _controller.DeleteOrder(order.id) as OkObjectResult;

        //     // Assert
        //     result.Should().NotBeNull();
        //     result.StatusCode.Should().Be(200);
        //     result.Value.Should().Be("Order has been deleted!");

        //     var deletedOrder = await DbContext.Order.FirstOrDefaultAsync(o => o.id == order.id);
        //     deletedOrder.Should().BeNull();
        // }

        [Fact]
        public async Task DeleteOrder_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var order = await DbContext.Order.FirstOrDefaultAsync(o => o.reference == "REF001");
            order.Should().NotBeNull();

            // Act
            var result = await _controller.DeleteOrder(order.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }
    }
}