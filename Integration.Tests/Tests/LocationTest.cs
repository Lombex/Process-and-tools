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
    public class LocationTests : IntegrationTestBase
    {
        private readonly LocationsController _controller;
        private readonly ILocationService _service;
        private readonly IAuthService _authService;

        public LocationTests()
        {
            _service = new LocationService(DbContext);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new LocationsController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.Location.RemoveRange(DbContext.Location);
            DbContext.Warehouse.RemoveRange(DbContext.Warehouse);
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            // Seed test warehouse data
            var warehouses = new List<WarehouseModel>
            {
                new WarehouseModel
                {
                    code = "WH001",
                    name = "Main Warehouse",
                    address = "123 Main St",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new WarehouseModel
                {
                    code = "WH002",
                    name = "Secondary Warehouse",
                    address = "456 Second St",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.Warehouse.AddRangeAsync(warehouses);
            await DbContext.SaveChangesAsync();

            var warehouse1 = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            var warehouse2 = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH002");

            // Seed test location data
            var locations = new List<LocationModel>
            {
                new LocationModel
                {
                    warehouse_id = warehouse1.id,
                    code = "LOC001",
                    name = "Zone A",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new LocationModel
                {
                    warehouse_id = warehouse1.id,
                    code = "LOC002",
                    name = "Zone B",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new LocationModel
                {
                    warehouse_id = warehouse2.id,
                    code = "LOC003",
                    name = "Zone C",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.Location.AddRangeAsync(locations);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_ReturnsAllLocations_WhenAuthorized()
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
            var locationsProperty = responseType.GetProperty("Location").GetValue(response) as IEnumerable<object>;

            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(3);
            totalPagesProperty.Should().Be(1);

            locationsProperty.Should().NotBeNull();
            var locations = locationsProperty.ToList();
            locations.Should().HaveCount(3);

            // Check specific locations
            var firstLocation = locations.First();
            var firstLocationType = firstLocation.GetType();
            (firstLocationType.GetProperty("Code").GetValue(firstLocation) as string).Should().Be("LOC001");
            
            var lastLocation = locations.Last();
            var lastLocationType = lastLocation.GetType();
            (lastLocationType.GetProperty("Code").GetValue(lastLocation) as string).Should().Be("LOC003");
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
        public async Task GetById_ReturnsLocation_WhenAuthorized()
        {
            // Arrange
            var location = await DbContext.Location.FirstOrDefaultAsync(l => l.code == "LOC001");
            location.Should().NotBeNull();

            // Act
            var result = await _controller.GetById(location.id) as OkObjectResult;
            var returnedLocation = result?.Value as LocationModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedLocation.Should().NotBeNull();
            returnedLocation.code.Should().Be("LOC001");
            returnedLocation.name.Should().Be("Zone A");
        }

        [Fact]
        public async Task GetById_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var location = await DbContext.Location.FirstOrDefaultAsync(l => l.code == "LOC001");
            location.Should().NotBeNull();

            // Act
            var result = await _controller.GetById(location.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Create_AddsNewLocation_WhenAuthorized()
        {
            // Arrange
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull();

            var newLocation = new LocationModel
            {
                warehouse_id = warehouse.id,
                code = "LOC004",
                name = "Zone D",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act
            var result = await _controller.Create(newLocation) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdLocation = result.Value as LocationModel;
            createdLocation.Should().NotBeNull();
            createdLocation.code.Should().Be("LOC004");
            createdLocation.name.Should().Be("Zone D");
        }

        [Fact]
        public async Task Create_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull();

            var newLocation = new LocationModel
            {
                warehouse_id = warehouse.id,
                code = "LOC004",
                name = "Zone D"
            };

            // Act
            var result = await _controller.Create(newLocation);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Update_UpdatesExistingLocation_WhenAuthorized()
        {
            // Arrange
            var location = await DbContext.Location.FirstOrDefaultAsync(l => l.code == "LOC001");
            location.Should().NotBeNull();

            var updateLocation = new LocationModel
            {
                warehouse_id = location.warehouse_id,
                code = "LOC001-UPD",
                name = "Updated Zone A"
            };

            // Act
            var result = await _controller.Update(location.id, updateLocation) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Location has been updated!");

            var updatedLocation = await DbContext.Location.FirstOrDefaultAsync(l => l.id == location.id);
            updatedLocation.Should().NotBeNull();
            updatedLocation.code.Should().Be("LOC001-UPD");
            updatedLocation.name.Should().Be("Updated Zone A");
        }

        [Fact]
        public async Task Update_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var location = await DbContext.Location.FirstOrDefaultAsync(l => l.code == "LOC001");
            location.Should().NotBeNull();

            var updateLocation = new LocationModel
            {
                code = "LOC001-UPD",
                name = "Updated Zone A"
            };

            // Act
            var result = await _controller.Update(location.id, updateLocation);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Delete_RemovesLocation_WhenAuthorized()
        {
            // Arrange
            var location = await DbContext.Location.FirstOrDefaultAsync(l => l.code == "LOC001");
            location.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(location.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Location has been deleted!");

            var deletedLocation = await DbContext.Location.FirstOrDefaultAsync(l => l.id == location.id);
            deletedLocation.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var location = await DbContext.Location.FirstOrDefaultAsync(l => l.code == "LOC001");
            location.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(location.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }
    }
}
