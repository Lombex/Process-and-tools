using CSharpAPI.Controllers;
using CSharpAPI.Models;
using CSharpAPI.Service;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Integration.Tests
{
    public class LocationTests : IntegrationTestBase
    {
        private readonly LocationsController _controller;
        private readonly ILocationService _service;

        public LocationTests()
        {
            _service = new LocationService(DbContext);
            _controller = new LocationsController(_service);

            // Seed test warehouse data
            var warehouse1 = new WarehouseModel
            {
                id = 1,
                code = "WH001",
                name = "Main Warehouse",
                address = "123 Main St",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            var warehouse2 = new WarehouseModel
            {
                id = 2,
                code = "WH002",
                name = "Secondary Warehouse",
                address = "456 Second St",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            DbContext.Warehouse.AddRange(warehouse1, warehouse2);

            // Seed test location data
            DbContext.Location.AddRange(
                new LocationModel
                {
                    id = 1,
                    warehouse_id = 1,
                    code = "LOC001",
                    name = "Zone A",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new LocationModel
                {
                    id = 2,
                    warehouse_id = 1,
                    code = "LOC002",
                    name = "Zone B",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new LocationModel
                {
                    id = 3,
                    warehouse_id = 2,
                    code = "LOC003",
                    name = "Zone C",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            );

            DbContext.SaveChanges();
        }

        [Fact]
        public async Task GetAll_ReturnsAllLocations()
        {
            // Act
            var result = await _controller.GetAll() as OkObjectResult;
            var locations = result?.Value as List<LocationModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            locations.Should().HaveCount(3);
            locations.Should().Contain(l => l.code == "LOC001");
            locations.Should().Contain(l => l.code == "LOC002");
            locations.Should().Contain(l => l.code == "LOC003");
        }

        [Fact]
        public async Task GetById_ReturnsLocation()
        {
            // Act
            var result = await _controller.GetById(1) as OkObjectResult;
            var location = result?.Value as LocationModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            location.Should().NotBeNull();
            location.code.Should().Be("LOC001");
            location.name.Should().Be("Zone A");
            location.warehouse_id.Should().Be(1);
        }

        [Fact]
        public async Task GetByWarehouseId_ReturnsLocations()
        {
            // Act
            var result = await _controller.GetByWarehouseId(1) as OkObjectResult;
            var locations = result?.Value as List<LocationModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            locations.Should().HaveCount(2);
            locations.Should().OnlyContain(l => l.warehouse_id == 1);
        }

        [Fact]
        public async Task Create_AddsNewLocation()
        {
            // Arrange
            var newLocation = new LocationModel
            {
                warehouse_id = 2,
                code = "LOC004",
                name = "Zone D"
            };

            // Act
            var result = await _controller.Create(newLocation) as CreatedAtActionResult;
            var locations = await _service.GetAll();

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            locations.Should().HaveCount(4);
            locations.Should().Contain(l => l.code == "LOC004");
        }

        [Fact]
        public async Task Update_UpdatesExistingLocation()
        {
            // Arrange
            var updatedLocation = new LocationModel
            {
                warehouse_id = 1,
                code = "LOC001-UPD",
                name = "Updated Zone A"
            };

            // Act
            var result = await _controller.Update(1, updatedLocation) as OkObjectResult;
            var location = await _service.GetById(1);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            location.code.Should().Be("LOC001-UPD");
            location.name.Should().Be("Updated Zone A");
        }

        [Fact]
        public async Task Update_ReturnsNotFound_ForInvalidId()
        {
            // Arrange
            var updatedLocation = new LocationModel
            {
                warehouse_id = 1,
                code = "INVALID",
                name = "Invalid Location"
            };

            // Act
            var result = await _controller.Update(99, updatedLocation) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);
            result.Value.Should().Be("Location 99 not found");
        }

        [Fact]
        public async Task Delete_RemovesLocation()
        {
            // Act
            var result = await _controller.Delete(1) as OkObjectResult;
            var locations = await _service.GetAll();

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            locations.Should().HaveCount(2);
            locations.Should().NotContain(l => l.id == 1);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_ForInvalidId()
        {
            // Act
            var result = await _controller.Delete(99) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);
            result.Value.Should().Be("Location 99 not found");
        }
    }
}