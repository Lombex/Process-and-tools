using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CShartpAPI.Controller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class OrdersControllerTests
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed initial data
            context.Order.AddRange(
                new OrderModel
                {
                    id = 1,
                    source_id = 101,
                    order_date = "2023-12-01",
                    request_date = "2023-12-02",
                    reference = "REF001",
                    reference_extra = "Extra details for REF001",
                    order_status = "Pending",
                    notes = "First order notes",
                    shipping_notes = "Deliver ASAP",
                    picking_notes = "Priority picking required",
                    warehouse_id = 1,
                    ship_to = 201,
                    bill_to = 301,
                    shipment_id = 1,
                    total_amount = 1000.0f,
                    total_discount = 50.0f,
                    total_tax = 100.0f,
                    total_surcharge = 10.0f,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    items = new List<Items>
                    {
                        new Items { item_id = "ITEM001", amount = 5 },
                        new Items { item_id = "ITEM002", amount = 3 }
                    }
                },
                new OrderModel
                {
                    id = 2,
                    source_id = 102,
                    order_date = "2023-12-05",
                    request_date = "2023-12-06",
                    reference = "REF002",
                    reference_extra = "Extra details for REF002",
                    order_status = "Shipped",
                    notes = "Second order notes",
                    shipping_notes = "Leave at the door",
                    picking_notes = "Normal priority",
                    warehouse_id = 2,
                    ship_to = 202,
                    bill_to = 302,
                    shipment_id = 2,
                    total_amount = 2000.0f,
                    total_discount = 100.0f,
                    total_tax = 200.0f,
                    total_surcharge = 20.0f,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    items = new List<Items>
                    {
                        new Items { item_id = "ITEM003", amount = 10 }
                    }
                }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAllOrders_ReturnsAllOrders()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new OrderService(dbContext);
            var controller = new OrdersControllers(service);

            var result = await controller.GetAllOrders() as OkObjectResult;
            var orders = result?.Value as List<OrderModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, orders.Count);
        }

        [Fact]
        public async Task GetOrdersById_ReturnsOrder()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new OrderService(dbContext);
            var controller = new OrdersControllers(service);

            var result = await controller.GetOrdersById(1) as OkObjectResult;
            var order = result?.Value as OrderModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(order);
            Xunit.Assert.Equal("REF001", order.reference);
        }

        [Fact]
        public async Task GetOrdersById_ReturnsNotFound()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new OrderService(dbContext);
            var controller = new OrdersControllers(service);

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.GetOrdersById(99));
            Xunit.Assert.Equal("Order not found!", exception.Message);
        }

        [Fact]
        public async Task GetItemFromOrderId_ReturnsItems()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new OrderService(dbContext);
            var controller = new OrdersControllers(service);

            var result = await controller.GetItemFromOrderId(1) as OkObjectResult;
            var items = result?.Value as List<Items>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task CreateOrder_AddsNewOrder()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new OrderService(dbContext);
            var controller = new OrdersControllers(service);

            var newOrder = new OrderModel
            {
                id = 3,
                source_id = 103,
                order_date = "2023-12-10",
                request_date = "2023-12-11",
                reference = "REF003",
                reference_extra = "Extra details for REF003",
                order_status = "Pending",
                notes = "Third order notes",
                shipping_notes = "Urgent delivery",
                picking_notes = "Handle with care",
                warehouse_id = 3,
                ship_to = 203,
                bill_to = 303,
                shipment_id = 3,
                total_amount = 3000.0f,
                total_discount = 150.0f,
                total_tax = 300.0f,
                total_surcharge = 30.0f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { item_id = "ITEM004", amount = 20 }
                }
            };

            var result = await controller.CreateOrder(newOrder) as CreatedAtActionResult;
            var orders = await dbContext.Order.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(3, orders.Count);
            Xunit.Assert.Contains(orders, o => o.reference == "REF003");
        }

        [Fact]
        public async Task UpdateOrders_UpdatesOrder()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new OrderService(dbContext);
            var controller = new OrdersControllers(service);

            var updatedOrder = new OrderModel
            {
                source_id = 101,
                reference = "REF001-Updated",
                order_status = "Completed",
                updated_at = DateTime.Now
            };

            var result = await controller.UpdateOrders(1, updatedOrder) as OkObjectResult;
            var order = await dbContext.Order.FirstOrDefaultAsync(o => o.id == 1);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(order);
            Xunit.Assert.Equal("REF001-Updated", order.reference);
        }

        [Fact]
        public async Task DeleteOrder_RemovesOrder()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new OrderService(dbContext);
            var controller = new OrdersControllers(service);

            var result = await controller.DeleteOrder(1) as OkObjectResult;
            var orders = await dbContext.Order.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(orders);
            Xunit.Assert.DoesNotContain(orders, o => o.id == 1);
        }
    }
}
