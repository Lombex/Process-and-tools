using CSharpAPI.Controllers;
using CSharpAPI.Models;
using CSharpAPI.Service;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Integration.Tests.Tests
{
    public class ShipmentsControllerTest : IntegrationTestBase
    {
        private readonly ShipmentsController _controller;
        private readonly IShipmentService _service;

        public ShipmentsControllerTest()
        {
            _service = new ShipmentService(DbContext);
            _controller = new ShipmentsController(_service);

            // Seed test shipment data
            DbContext.Shipment.AddRange(
                new ShipmentModel
                {
                    id = 1,
                    carrier_code = "CARRIER001",
                    shipment_status = "Pending",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ShipmentModel
                {
                    id = 2,
                    carrier_code = "CARRIER002",
                    shipment_status = "Completed",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            );

            DbContext.SaveChanges();
        }

        [Fact]
        public async Task GetAll_ReturnsAllShipments()
        {
            // Act
            var result = await _controller.GetAll() as OkObjectResult;
            var shipments = result?.Value as List<ShipmentModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            shipments.Should().HaveCount(2);
            shipments.Should().Contain(s => s.carrier_code == "CARRIER001");
            shipments.Should().Contain(s => s.carrier_code == "CARRIER002");
        }

        [Fact]
        public async Task GetById_ReturnsShipment()
        {
            // Act
            var result = await _controller.GetById(1) as OkObjectResult;
            var shipment = result?.Value as ShipmentModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            shipment.Should().NotBeNull();
            shipment.carrier_code.Should().Be("CARRIER001");
        }

        [Fact]
        public async Task GetById_ThrowsException_ForInvalidId()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.GetById(99);
            });
        }

        [Fact]
        public async Task Create_AddsNewShipment()
        {
            // Arrange
            var newShipment = new ShipmentModel
            {
                carrier_code = "CARRIER003",
                shipment_status = "Pending",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act
            var result = await _controller.Create(newShipment) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdShipment = result.Value as ShipmentModel;
            createdShipment.Should().NotBeNull();
            createdShipment.carrier_code.Should().Be("CARRIER003");
        }

        [Fact]
        public async Task Update_UpdatesExistingShipment()
        {
            // Arrange
            var updatedShipment = new ShipmentModel
            {
                carrier_code = "CARRIER001-UPD",
                shipment_status = "Completed"
            };

            // Act
            var result = await _controller.Update(1, updatedShipment) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Shipment has been updated!");
        }

        [Fact]
        public async Task Update_ThrowsException_ForInvalidId()
        {
            // Arrange
            var updatedShipment = new ShipmentModel
            {
                carrier_code = "CARRIER099",
                shipment_status = "Pending"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.Update(99, updatedShipment);
            });
        }

        [Fact]
        public async Task Delete_RemovesShipment()
        {
            // Act
            var result = await _controller.Delete(1) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Shipment has been deleted!");
        }

        [Fact]
        public async Task Delete_ThrowsException_ForInvalidId()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.Delete(99);
            });
        }

        [Fact]
        public async Task GetItems_ReturnsItems()
        {
            // Act
            var result = await _controller.GetItems(1) as OkObjectResult;
            var items = result?.Value as List<Items>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            items.Should().NotBeNull();
        }

        [Fact]
        public async Task GetItems_ThrowsException_ForInvalidId()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.GetItems(99);
            });
        }
    }
}