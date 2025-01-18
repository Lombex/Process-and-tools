using CSharpAPI.Controller;
using CSharpAPI.Models;
using CSharpAPI.Service;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Integration.Tests
{
    public class WarehouseTests : IntegrationTestBase
    {
        private readonly WarehousesController _controller;
        private readonly IWarehouseService _service;

        public WarehouseTests()
        {
            _service = new WarehouseService(DbContext);
            _controller = new WarehousesController(_service);

            // Seed test warehouse data
            DbContext.Warehouse.AddRange(
                new WarehouseModel
                {
                    id = 1,
                    code = "WH001",
                    name = "Main Warehouse",
                    address = "123 Main St",
                    zip = "12345",
                    city = "Test City",
                    province = "Test Province",
                    country = "Test Country",
                    contact = new Contact
                    {
                        name = "John Doe",
                        phone = "123-456-7890",
                        email = "john@test.com"
                    },
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new WarehouseModel
                {
                    id = 2,
                    code = "WH002",
                    name = "Secondary Warehouse",
                    address = "456 Second St",
                    zip = "67890",
                    city = "Other City",
                    province = "Other Province",
                    country = "Other Country",
                    contact = new Contact
                    {
                        name = "Jane Smith",
                        phone = "098-765-4321",
                        email = "jane@test.com"
                    },
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            );

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
                }
            );

            DbContext.SaveChanges();
        }

        [Fact]
        public async Task GetAll_ReturnsAllWarehouses()
        {
            // Act
            var result = await _controller.GetAll() as OkObjectResult;
            var warehouses = result?.Value as List<WarehouseModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            warehouses.Should().HaveCount(2);
            warehouses.Should().Contain(w => w.code == "WH001");
            warehouses.Should().Contain(w => w.code == "WH002");
        }

        [Fact]
        public async Task Get_ReturnsWarehouseById()
        {
            // Act
            var result = await _controller.Get(1) as OkObjectResult;
            var warehouse = result?.Value as WarehouseModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            warehouse.Should().NotBeNull();
            warehouse.code.Should().Be("WH001");
            warehouse.name.Should().Be("Main Warehouse");
        }

        [Fact]
        public async Task Get_ReturnsNotFound_ForInvalidId()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _controller.Get(99)
            );
            exception.Message.Should().Be("Warehouse not found!");
        }

        [Fact]
        public async Task Post_CreatesNewWarehouse()
        {
            // Arrange
            var newWarehouse = new WarehouseModel
            {
                code = "WH003",
                name = "New Warehouse",
                address = "789 New St",
                zip = "11111",
                city = "New City",
                province = "New Province",
                country = "New Country",
                contact = new Contact
                {
                    name = "New Contact",
                    phone = "111-111-1111",
                    email = "new@test.com"
                }
            };

            // Act
            var result = await _controller.Post(newWarehouse) as CreatedAtActionResult;
            var warehouses = await _service.GetAllWarehouses();

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            warehouses.Should().HaveCount(3);
            warehouses.Should().Contain(w => w.code == "WH003");
        }

        [Fact]
        public async Task Put_UpdatesExistingWarehouse()
        {
            // Arrange
            var updatedWarehouse = new WarehouseModel
            {
                code = "WH001-UPD",
                name = "Updated Warehouse",
                address = "Updated Address",
                zip = "99999",
                city = "Updated City",
                province = "Updated Province",
                country = "Updated Country",
                contact = new Contact
                {
                    name = "Updated Contact",
                    phone = "999-999-9999",
                    email = "updated@test.com"
                }
            };

            // Act
            var result = await _controller.Put(1, updatedWarehouse) as OkObjectResult;
            var warehouse = await _service.GetWarehouseById(1);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            warehouse.Should().NotBeNull();
            warehouse.code.Should().Be("WH001-UPD");
            warehouse.name.Should().Be("Updated Warehouse");
        }

        [Fact]
        public async Task LocationFromWarehouseID_ReturnsLocations()
        {
            // Act
            var result = await _controller.LocationFromWarehouseID(1) as OkObjectResult;
            var locations = result?.Value as List<LocationModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            locations.Should().HaveCount(2);
            locations.Should().Contain(l => l.code == "LOC001");
            locations.Should().Contain(l => l.code == "LOC002");
        }

        [Fact]
        public async Task Delete_RemovesWarehouse()
        {
            // Act
            var result = await _controller.Delete(1) as OkObjectResult;
            var warehouses = await _service.GetAllWarehouses();

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            warehouses.Should().HaveCount(1);
            warehouses.Should().NotContain(w => w.id == 1);
        }
    }
}