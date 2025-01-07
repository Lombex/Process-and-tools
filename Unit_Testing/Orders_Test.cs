using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class OrderServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public OrderServiceTests()
        {
            // Set up In-Memory database options
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "OrderTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task GetAllOrders_ReturnsListOfOrders()
        {
            // Arrange: Create a list of orders based on the provided data
            var orderList = new List<OrderModel>
            {
                new OrderModel
                {
                    id = 1,
                    source_id = 33,
                    order_date = "2019-04-03T11:33:15Z",
                    request_date = "2019-04-07T11:33:15Z",
                    reference = "ORD00001",
                    reference_extra = "Bedreven arm straffen bureau.",
                    order_status = "Delivered",
                    notes = "Voedsel vijf vork heel.",
                    shipping_notes = "Buurman betalen plaats bewolkt.",
                    picking_notes = "Ademen fijn volgorde scherp aardappel op leren.",
                    warehouse_id = 18,
                    ship_to = 4562,
                    bill_to = 7863,
                    shipment_id = 1,
                    total_amount = 9905.13f,
                    total_discount = 150.77f,
                    total_tax = 372.72f,
                    total_surcharge = 77.6f,
                    created_at = DateTime.Parse("2019-04-03T11:33:15"),
                    updated_at = DateTime.Parse("2019-04-05T07:33:15"),
                    items = new List<Items>
                    {
                        new Items { item_id = "P007435", amount = 23 },
                        new Items { item_id = "P009557", amount = 1 },
                        new Items { item_id = "P009553", amount = 50 },
                        new Items { item_id = "P010015", amount = 16 }
                    }
                }
            };

            using (var context = CreateDbContext())
            {
                context.Order.AddRange(orderList);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve all orders
            List<OrderModel> result;
            using (var context = CreateDbContext())
            {
                var service = new OrderService(context);
                result = await service.GetAllOrders();
            }

            // Assert: Ensure that the result is not null and contains the expected number of orders
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }

        [Fact]
        public async Task GetOrderById_ValidId_ReturnsOrder()
        {
            // Arrange: Add order to the database
            var order = new OrderModel
            {
                id = 1,
                source_id = 33,
                order_date = "2019-04-03T11:33:15Z",
                request_date = "2019-04-07T11:33:15Z",
                reference = "ORD00001",
                reference_extra = "Bedreven arm straffen bureau.",
                order_status = "Delivered",
                notes = "Voedsel vijf vork heel.",
                shipping_notes = "Buurman betalen plaats bewolkt.",
                picking_notes = "Ademen fijn volgorde scherp aardappel op leren.",
                warehouse_id = 18,
                ship_to = 4562,
                bill_to = 7863,
                shipment_id = 1,
                total_amount = 9905.13f,
                total_discount = 150.77f,
                total_tax = 372.72f,
                total_surcharge = 77.6f,
                created_at = DateTime.Parse("2019-04-03T11:33:15"),
                updated_at = DateTime.Parse("2019-04-05T07:33:15"),
                items = new List<Items>
                {
                    new Items { item_id = "P007435", amount = 23 },
                    new Items { item_id = "P009557", amount = 1 }
                }
            };

            using (var context = CreateDbContext())
            {
                context.Order.Add(order);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve the order by ID
            OrderModel result;
            using (var context = CreateDbContext())
            {
                var service = new OrderService(context);
                result = await service.GetOrderById(1);
            }

            // Assert: Check if the returned order matches the expected data
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("ORD00001", result.reference);
            Assert.Equal("Delivered", result.order_status);
        }

        [Fact]
        public async Task CreateOrder_ValidOrder_CreatesOrder()
        {
            // Arrange: Create a new order
            var newOrder = new OrderModel
            {
                source_id = 33,
                order_date = "2019-04-03T11:33:15Z",
                request_date = "2019-04-07T11:33:15Z",
                reference = "ORD00002",
                reference_extra = "Test Order",
                order_status = "Pending",
                notes = "Test Notes",
                shipping_notes = "Shipping Test Notes",
                picking_notes = "Picking Test Notes",
                warehouse_id = 18,
                ship_to = 4562,
                bill_to = 7863,
                shipment_id = 1,
                total_amount = 1500.00f,
                total_discount = 50.00f,
                total_tax = 100.00f,
                total_surcharge = 20.00f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { item_id = "P000123", amount = 10 },
                    new Items { item_id = "P000124", amount = 5 }
                }
            };

            // Act: Add the order to the database
            using (var context = CreateDbContext())
            {
                var service = new OrderService(context);
                await service.CreateOrder(newOrder);
            }

            // Assert: Ensure the new order is added to the database
            using (var context = CreateDbContext())
            {
                var result = await context.Order.FirstOrDefaultAsync(o => o.reference == "ORD00002");
                Assert.NotNull(result);
                Assert.Equal("ORD00002", result.reference);
            }
        }

        [Fact]
        public async Task UpdateOrders_ValidOrder_UpdatesOrder()
        {
            // Arrange: Create and save an order
            var order = new OrderModel
            {
                id = 1,
                source_id = 33,
                order_date = "2019-04-03T11:33:15Z",
                request_date = "2019-04-07T11:33:15Z",
                reference = "ORD00001",
                reference_extra = "Bedreven arm straffen bureau.",
                order_status = "Delivered",
                total_amount = 9905.13f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { item_id = "P007435", amount = 23 }
                }
            };

            using (var context = CreateDbContext())
            {
                context.Order.Add(order);
                await context.SaveChangesAsync();
            }

            var updatedOrder = new OrderModel
            {
                source_id = 33,
                order_date = "2019-04-03T11:33:15Z",
                request_date = "2019-04-07T11:33:15Z",
                reference = "ORD00001",
                reference_extra = "Updated Reference",
                order_status = "Shipped",
                total_amount = 10500.00f, // Updated total amount
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = order.items
            };

            // Act: Update the order
            using (var context = CreateDbContext())
            {
                var service = new OrderService(context);
                await service.UpdateOrders(1, updatedOrder);
            }

            // Assert: Verify that the order was updated
            using (var context = CreateDbContext())
            {
                var result = await context.Order.FirstOrDefaultAsync(o => o.id == 1);
                Assert.NotNull(result);
                Assert.Equal("Updated Reference", result.reference_extra);
                Assert.Equal(10500.00f, result.total_amount);
            }
        }

        [Fact]
        public async Task DeleteOrder_ValidOrder_DeletesOrder()
        {
            // Arrange: Create and add an order
            var order = new OrderModel
            {
                id = 1,
                source_id = 33,
                order_date = "2019-04-03T11:33:15Z",
                request_date = "2019-04-07T11:33:15Z",
                reference = "ORD00001",
                reference_extra = "Bedreven arm straffen bureau.",
                order_status = "Delivered",
                total_amount = 9905.13f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { item_id = "P007435", amount = 23 }
                }
            };

            using (var context = CreateDbContext())
            {
                context.Order.Add(order);
                await context.SaveChangesAsync();
            }

            // Act: Delete the order
            using (var context = CreateDbContext())
            {
                var service = new OrderService(context);
                await service.DeleteOrder(1);
            }

            // Assert: Ensure the order is deleted
            using (var context = CreateDbContext())
            {
                var result = await context.Order.FirstOrDefaultAsync(o => o.id == 1);
                Assert.Null(result); // The order should be deleted
            }
        }
    }
}
