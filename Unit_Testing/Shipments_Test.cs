using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class ShipmentServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public ShipmentServiceTests()
        {
            // Set up In-Memory database options
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "ShipmentTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task GetAll_ReturnsListOfShipments()
        {
            // Arrange
            var shipmentList = new List<ShipmentModel>
            {
                new ShipmentModel
                {
                    id = 1,
                    order_id = 1,
                    source_id = 1,
                    order_date = "2023-01-01",
                    request_date = "2023-01-02",
                    shipment_date = "2023-01-03",
                    shipment_type = "Air",
                    shipment_status = "Shipped",
                    total_package_count = 5,
                    total_package_weight = 12.5f,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new ShipmentModel
                {
                    id = 2,
                    order_id = 2,
                    source_id = 2,
                    order_date = "2023-02-01",
                    request_date = "2023-02-02",
                    shipment_date = "2023-02-03",
                    shipment_type = "Sea",
                    shipment_status = "Pending",
                    total_package_count = 3,
                    total_package_weight = 8.2f,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                }
            };

            using (var context = CreateDbContext())
            {
                context.Shipment.AddRange(shipmentList);
                await context.SaveChangesAsync();
            }

            // Act
            List<ShipmentModel> result;
            using (var context = CreateDbContext())
            {
                var service = new ShipmentService(context);
                result = await service.GetAll();
            }

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsShipment()
        {
            // Arrange
            var shipment = new ShipmentModel
            {
                id = 1,
                order_id = 1,
                source_id = 1,
                order_date = "2023-01-01",
                request_date = "2023-01-02",
                shipment_date = "2023-01-03",
                shipment_type = "Air",
                shipment_status = "Shipped",
                total_package_count = 5,
                total_package_weight = 12.5f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            using (var context = CreateDbContext())
            {
                context.Shipment.Add(shipment);
                await context.SaveChangesAsync();
            }

            // Act
            ShipmentModel result;
            using (var context = CreateDbContext())
            {
                var service = new ShipmentService(context);
                result = await service.GetById(1);
            }

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Air", result.shipment_type);
        }

        [Fact]
        public async Task Add_ValidShipment_AddsShipment()
        {
            // Arrange
            var newShipment = new ShipmentModel
            {
                order_id = 3,
                source_id = 3,
                order_date = "2023-03-01",
                request_date = "2023-03-02",
                shipment_date = "2023-03-03",
                shipment_type = "Ground",
                shipment_status = "In Progress",
                total_package_count = 4,
                total_package_weight = 15.0f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            // Act
            using (var context = CreateDbContext())
            {
                var service = new ShipmentService(context);
                await service.Add(newShipment);
            }

            // Assert
            using (var context = CreateDbContext())
            {
                var result = await context.Shipment.FirstOrDefaultAsync(s => s.order_id == 3);
                Assert.NotNull(result);
                Assert.Equal("Ground", result.shipment_type);
            }
        }

        [Fact]
        public async Task Update_ValidShipment_UpdatesShipment()
        {
            // Arrange
            var shipment = new ShipmentModel
            {
                id = 1,
                order_id = 1,
                source_id = 1,
                order_date = "2023-01-01",
                request_date = "2023-01-02",
                shipment_date = "2023-01-03",
                shipment_type = "Air",
                shipment_status = "Shipped",
                total_package_count = 5,
                total_package_weight = 12.5f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            using (var context = CreateDbContext())
            {
                context.Shipment.Add(shipment);
                await context.SaveChangesAsync();
            }

            var updatedShipment = new ShipmentModel
            {
                order_id = 1,
                source_id = 1,
                order_date = "2023-01-01",
                request_date = "2023-01-02",
                shipment_date = "2023-01-04", // Updated shipment date
                shipment_type = "Sea",
                shipment_status = "Shipped",
                total_package_count = 5,
                total_package_weight = 12.5f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            // Act
            using (var context = CreateDbContext())
            {
                var service = new ShipmentService(context);
                await service.Update(1, updatedShipment);
            }

            // Assert
            using (var context = CreateDbContext())
            {
                var result = await context.Shipment.FirstOrDefaultAsync(s => s.id == 1);
                Assert.NotNull(result);
                Assert.Equal("Sea", result.shipment_type);
                Assert.Equal("2023-01-04", result.shipment_date);
            }
        }

        [Fact]
        public async Task Delete_ValidShipment_DeletesShipment()
        {
            // Arrange
            var shipment = new ShipmentModel
            {
                id = 1,
                order_id = 1,
                source_id = 1,
                order_date = "2023-01-01",
                request_date = "2023-01-02",
                shipment_date = "2023-01-03",
                shipment_type = "Air",
                shipment_status = "Shipped",
                total_package_count = 5,
                total_package_weight = 12.5f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            using (var context = CreateDbContext())
            {
                context.Shipment.Add(shipment);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = CreateDbContext())
            {
                var service = new ShipmentService(context);
                await service.Delete(1);
            }

            // Assert
            using (var context = CreateDbContext())
            {
                var result = await context.Shipment.FirstOrDefaultAsync(s => s.id == 1);
                Assert.Null(result); // The shipment should be deleted
            }
        }
    }
}
