using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Controller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class Warehouses_Testing
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed initial data
            context.Warehouse.AddRange(
                new WarehouseModel
                {
                    id = 1,
                    code = "WH1",
                    name = "Warehouse 1",
                    address = "123 Test St",
                    zip = "12345",
                    city = "Testville",
                    province = "Test Province",
                    country = "Test Country",
                    contact = {},
                    updated_at = DateTime.Now
                },
                new WarehouseModel
                {
                    id = 2,
                    code = "WH2",
                    name = "Warehouse 2",
                    address = "456 Another St",
                    zip = "67890",
                    city = "Anotherville",
                    province = "Another Province",
                    country = "Another Country",
                    contact = {},
                    updated_at = DateTime.Now
                }
            );

            context.Location.AddRange(
                new LocationModel
                {
                    id = 1,
                    warehouse_id = 1,
                    name = "Location 1"
                },
                new LocationModel
                {
                    id = 2,
                    warehouse_id = 1,
                    name = "Location 2"
                }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAll_ReturnsAllWarehouses()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new WarehouseService(dbContext);
            var controller = new WarehousesController(service);

            var result = await controller.GetAll() as OkObjectResult;
            var warehouses = result?.Value as List<WarehouseModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, warehouses.Count);
        }

        [Fact]
        public async Task Get_ReturnsWarehouseById()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new WarehouseService(dbContext);
            var controller = new WarehousesController(service);

            var result = await controller.Get(1) as OkObjectResult;
            var warehouse = result?.Value as WarehouseModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(warehouse);
            Xunit.Assert.Equal("Warehouse 1", warehouse.name);
        }

        [Fact]
        public async Task Get_ReturnsNotFoundForInvalidId()
        {
            // Arrange
            var dbContext = GetInMemoryDatabaseContext();
            var service = new WarehouseService(dbContext);
            var controller = new WarehousesController(service);

            // Act
            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.Get(99));

            // Assert
            Xunit.Assert.Equal("Warehouse not found!", exception.Message);
        }


        [Fact]
        public async Task LocationFromWarehouseID_ReturnsLocations()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new WarehouseService(dbContext);
            var controller = new WarehousesController(service);

            var result = await controller.LocationFromWarehouseID(1) as OkObjectResult;
            var locations = result?.Value as List<LocationModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, locations.Count);
        }

        [Fact]
        public async Task Post_AddsNewWarehouse()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new WarehouseService(dbContext);
            var controller = new WarehousesController(service);

            var newWarehouse = new WarehouseModel
            {
                id = 3,
                code = "WH3",
                name = "Warehouse 3",
                address = "789 New St",
                zip = "11111",
                city = "Newville",
                province = "New Province",
                country = "New Country",
                contact = { },
                updated_at = DateTime.Now
            };

            var result = await controller.Post(newWarehouse) as CreatedAtActionResult;
            var warehouses = await dbContext.Warehouse.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(3, warehouses.Count);
            Xunit.Assert.Contains(warehouses, w => w.name == "Warehouse 3");
        }

        [Fact]
        public async Task Put_UpdatesWarehouse()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new WarehouseService(dbContext);
            var controller = new WarehousesController(service);

            var updatedWarehouse = new WarehouseModel
            {
                code = "WH1-Updated",
                name = "Warehouse 1 Updated",
                address = "Updated Address",
                zip = "99999",
                city = "Updated City",
                province = "Updated Province",
                country = "Updated Country",
                contact = {},
                updated_at = DateTime.Now
            };

            var result = await controller.Put(1, updatedWarehouse) as OkObjectResult;
            var warehouse = await dbContext.Warehouse.FirstOrDefaultAsync(w => w.id == 1);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(warehouse);
            Xunit.Assert.Equal("Warehouse 1 Updated", warehouse.name);
        }

        [Fact]
        public async Task Delete_RemovesWarehouse()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new WarehouseService(dbContext);
            var controller = new WarehousesController(service);

            var result = await controller.Delete(1) as OkObjectResult;
            var warehouses = await dbContext.Warehouse.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.  Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(warehouses);
            Xunit.Assert.DoesNotContain(warehouses, w => w.id == 1);
        }
    }
}
