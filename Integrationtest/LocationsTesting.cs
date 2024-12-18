using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class LocationsControllerTests
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed initial data
            context.Location.AddRange(
                new LocationModel
                {
                    id = 1,
                    warehouse_id = 101,
                    code = "LOC001",
                    name = "Location One",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new LocationModel
                {
                    id = 2,
                    warehouse_id = 102,
                    code = "LOC002",
                    name = "Location Two",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new LocationModel
                {
                    id = 3,
                    warehouse_id = 101,
                    code = "LOC003",
                    name = "Location Three",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAll_ReturnsAllLocations()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new LocationService(dbContext);
            var controller = new LocationsController(service);

            var result = await controller.GetAll() as OkObjectResult;
            var locations = result?.Value as List<LocationModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(3, locations.Count);
        }

        [Fact]
        public async Task GetById_ReturnsLocation()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new LocationService(dbContext);
            var controller = new LocationsController(service);

            var result = await controller.GetById(1) as OkObjectResult;
            var location = result?.Value as LocationModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(location);
            Xunit.Assert.Equal("Location One", location.name);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new LocationService(dbContext);
            var controller = new LocationsController(service);

            var result = await controller.GetById(99) as NotFoundObjectResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(404, result.StatusCode);
            Xunit.Assert.Equal("Location 99 not found", result.Value);
        }

        [Fact]
        public async Task GetByWarehouseId_ReturnsLocationsForWarehouse()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new LocationService(dbContext);
            var controller = new LocationsController(service);

            var result = await controller.GetByWarehouseId(101) as OkObjectResult;
            var locations = result?.Value as List<LocationModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, locations.Count);
        }

        [Fact]
        public async Task Create_AddsNewLocation()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new LocationService(dbContext);
            var controller = new LocationsController(service);

            var newLocation = new LocationModel
            {
                id = 4,
                warehouse_id = 103,
                code = "LOC004",
                name = "Location Four",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var result = await controller.Create(newLocation) as CreatedAtActionResult;
            var locations = await dbContext.Location.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(4, locations.Count);
            Xunit.Assert.Contains(locations, l => l.name == "Location Four");
        }

        [Fact]
        public async Task Update_UpdatesExistingLocation()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new LocationService(dbContext);
            var controller = new LocationsController(service);

            var updatedLocation = new LocationModel
            {
                warehouse_id = 101,
                code = "LOC001-Updated",
                name = "Location One Updated",
                updated_at = DateTime.Now
            };

            var result = await controller.Update(1, updatedLocation) as OkObjectResult;
            var location = await dbContext.Location.FirstOrDefaultAsync(l => l.id == 1);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(location);
            Xunit.Assert.Equal("Location One Updated", location.name);
        }

        [Fact]
        public async Task Update_ReturnsNotFoundForInvalidId()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new LocationService(dbContext);
            var controller = new LocationsController(service);

            var updatedLocation = new LocationModel
            {
                warehouse_id = 103,
                code = "LOC999",
                name = "Invalid Location",
                updated_at = DateTime.Now
            };

            var result = await controller.Update(99, updatedLocation) as NotFoundObjectResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(404, result.StatusCode);
            Xunit.Assert.Equal("Location 99 not found", result.Value);
        }

        [Fact]
        public async Task Delete_RemovesLocation()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new LocationService(dbContext);
            var controller = new LocationsController(service);

            var result = await controller.Delete(1) as OkObjectResult;
            var locations = await dbContext.Location.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, locations.Count);
            Xunit.Assert.DoesNotContain(locations, l => l.id == 1);
        }

        [Fact]
        public async Task Delete_ReturnsNotFoundForInvalidId()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new LocationService(dbContext);
            var controller = new LocationsController(service);

            var result = await controller.Delete(99) as NotFoundObjectResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(404, result.StatusCode);
            Xunit.Assert.Equal("Location 99 not found", result.Value);
        }
    }
}
