using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class ShipmentsControllerTests
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed shipments
            context.Shipment.AddRange(
                new ShipmentModel
                {
                    id = 1,
                    order_id = 1,
                    source_id = 1,
                    order_date = "2023-12-01",
                    request_date = "2023-12-02",
                    shipment_date = "2023-12-03",
                    shipment_type = "Air",
                    shipment_status = "Pending",
                    notes = "Handle with care",
                    carrier_code = "CC001",
                    carrier_description = "Air Freight",
                    service_code = "SC001",
                    payment_type = "Prepaid",
                    transfer_mode = "Direct",
                    total_package_count = 5,
                    total_package_weight = 50.5f,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    items = new List<Items>
                    {
                new Items { item_id = "ITEM001", amount = 10 },
                new Items { item_id = "ITEM002", amount = 5 }
                    }
                },
                new ShipmentModel
                {
                    id = 2,
                    order_id = 2,
                    source_id = 2,
                    order_date = "2023-12-04",
                    request_date = "2023-12-05",
                    shipment_date = "2023-12-06",
                    shipment_type = "Ground",
                    shipment_status = "Shipped",
                    notes = "Fragile items",
                    carrier_code = "CC002",
                    carrier_description = "Ground Freight",
                    service_code = "SC002",
                    payment_type = "Collect",
                    transfer_mode = "Warehouse",
                    total_package_count = 10,
                    total_package_weight = 100.0f,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    items = new List<Items>
                    {
                new Items { item_id = "ITEM003", amount = 15 }
                    }
                }
            );

            // Seed orders
            context.Order.AddRange(
                new OrderModel
                {
                    id = 1,
                    source_id = 1,
                    order_date = "2023-11-30",
                    request_date = "2023-12-01",
                    reference = "REF001",
                    reference_extra = "Extra details for REF001",
                    order_status = "Pending",
                    notes = "First order",
                    shipping_notes = "Deliver ASAP",
                    picking_notes = "High priority",
                    warehouse_id = 1,
                    ship_to = 101,
                    bill_to = 201,
                    shipment_id = 1,
                    total_amount = 500.0f,
                    total_discount = 50.0f,
                    total_tax = 25.0f,
                    total_surcharge = 5.0f,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    items = new List<Items>
                    {
                new Items { item_id = "ITEM001", amount = 2 },
                new Items { item_id = "ITEM002", amount = 1 }
                    }
                },
                new OrderModel
                {
                    id = 2,
                    source_id = 2,
                    order_date = "2023-12-01",
                    request_date = "2023-12-02",
                    reference = "REF002",
                    reference_extra = "Extra details for REF002",
                    order_status = "Shipped",
                    notes = "Second order",
                    shipping_notes = "Leave at the door",
                    picking_notes = "Normal priority",
                    warehouse_id = 2,
                    ship_to = 102,
                    bill_to = 202,
                    shipment_id = 2,
                    total_amount = 1000.0f,
                    total_discount = 100.0f,
                    total_tax = 50.0f,
                    total_surcharge = 10.0f,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    items = new List<Items>
                    {
                new Items { item_id = "ITEM003", amount = 3 }
                    }
                }
            );

            context.SaveChanges();
            return context;
        }


        [Fact]
        public async Task GetAll_ReturnsAllShipments()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ShipmentService(dbContext);
            var controller = new ShipmentsController(service);

            var result = await controller.GetAll() as OkObjectResult;
            var shipments = result?.Value as List<ShipmentModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, shipments.Count);
        }

        [Fact]
        public async Task GetById_ReturnsShipment()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ShipmentService(dbContext);
            var controller = new ShipmentsController(service);

            var result = await controller.GetById(1) as OkObjectResult;
            var shipment = result?.Value as ShipmentModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(shipment);
            Xunit.Assert.Equal("Pending", shipment.shipment_status);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ShipmentService(dbContext);
            var controller = new ShipmentsController(service);

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.GetById(99));
            Xunit.Assert.Equal("Shipment not found!", exception.Message);
        }

        [Fact]
        public async Task GetItems_ReturnsItems()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ShipmentService(dbContext);
            var controller = new ShipmentsController(service);

            var result = await controller.GetItems(1) as OkObjectResult;
            var items = result?.Value as List<Items>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task GetOrderByShipmentId_ReturnsOrder()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ShipmentService(dbContext);
            var controller = new ShipmentsController(service);

            var result = await controller.GetOrderByShipmentId(1) as OkObjectResult;
            var order = result?.Value as OrderModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(order);
        }

        [Fact]
        public async Task Create_AddsNewShipment()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ShipmentService(dbContext);
            var controller = new ShipmentsController(service);

            var newShipment = new ShipmentModel
            {
                id = 3,
                order_id = 1003,
                source_id = 3,
                order_date = "2023-12-07",
                request_date = "2023-12-08",
                shipment_date = "2023-12-09",
                shipment_type = "Sea",
                shipment_status = "Pending",
                notes = "Bulk shipment",
                carrier_code = "CC003",
                carrier_description = "Sea Freight",
                service_code = "SC003",
                payment_type = "Prepaid",
                transfer_mode = "Warehouse",
                total_package_count = 20,
                total_package_weight = 200.0f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var result = await controller.Create(newShipment) as CreatedAtActionResult;
            var shipments = await dbContext.Shipment.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(3, shipments.Count);
            Xunit.Assert.Contains(shipments, s => s.id == 3);
        }

        [Fact]
        public async Task Update_UpdatesShipment()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ShipmentService(dbContext);
            var controller = new ShipmentsController(service);

            var updatedShipment = new ShipmentModel
            {
                shipment_status = "Delivered",
                total_package_count = 15,
                updated_at = DateTime.Now
            };

            var result = await controller.Update(1, updatedShipment) as OkObjectResult;
            var shipment = await dbContext.Shipment.FirstOrDefaultAsync(s => s.id == 1);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(shipment);
            Xunit.Assert.Equal("Delivered", shipment.shipment_status);
        }

        [Fact]
        public async Task Delete_RemovesShipment()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ShipmentService(dbContext);
            var controller = new ShipmentsController(service);

            var result = await controller.Delete(1) as OkObjectResult;
            var shipments = await dbContext.Shipment.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(shipments);
            Xunit.Assert.DoesNotContain(shipments, s => s.id == 1);
        }
    }
}
