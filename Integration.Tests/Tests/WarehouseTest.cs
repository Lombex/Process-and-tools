using CSharpAPI.Controller;
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
    public class WarehouseTest : IntegrationTestBase
    {
        private readonly WarehouseController _controller;
        private readonly IWarehouseService _service;
        private readonly IAuthService _authService;

        public WarehouseTest()
        {
            _service = new WarehouseService(DbContext);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new WarehouseController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.Warehouse.RemoveRange(DbContext.Warehouse);
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var warehouses = new List<WarehouseModel>
            {
                new WarehouseModel
                {
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
                        email = "john@warehouse.com"
                    },
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new WarehouseModel
                {
                    code = "WH002",
                    name = "Secondary Warehouse",
                    address = "456 Second St",
                    zip = "67890",
                    city = "Test City 2",
                    province = "Test Province 2",
                    country = "Test Country",
                    contact = new Contact
                    {
                        name = "Jane Smith",
                        phone = "098-765-4321",
                        email = "jane@warehouse.com"
                    },
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.Warehouse.AddRangeAsync(warehouses);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_ReturnsAllWarehouses_WhenAuthorized()
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
            var warehousesProperty = responseType.GetProperty("Warehouses").GetValue(response) as IEnumerable<object>;

            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(2);
            totalPagesProperty.Should().Be(1);

            warehousesProperty.Should().NotBeNull();
            var warehouses = warehousesProperty.ToList();
            warehouses.Should().HaveCount(2);

            // Check first warehouse
            var firstWarehouse = warehouses.First();
            var firstWarehouseType = firstWarehouse.GetType();
            (firstWarehouseType.GetProperty("Code").GetValue(firstWarehouse) as string).Should().Be("WH001");
            
            // Check second warehouse
            var secondWarehouse = warehouses.Last();
            var secondWarehouseType = secondWarehouse.GetType();
            (secondWarehouseType.GetProperty("Code").GetValue(secondWarehouse) as string).Should().Be("WH002");
        }

        [Fact]
        public async Task GetAll_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            // Act
            var result = await _controller.GetAll(0);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }

        [Fact]
        public async Task Get_ReturnsWarehouse_WhenAuthorized()
        {
            // Arrange
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull();

            // Act
            var result = await _controller.Get(warehouse.id) as OkObjectResult;
            var returnedWarehouse = result?.Value as WarehouseModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedWarehouse.Should().NotBeNull();
            returnedWarehouse.code.Should().Be("WH001");
            returnedWarehouse.name.Should().Be("Main Warehouse");
            returnedWarehouse.contact.Should().NotBeNull();
            returnedWarehouse.contact.name.Should().Be("John Doe");
        }

        [Fact]
        public async Task Get_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull();

            // Act
            var result = await _controller.Get(warehouse.id);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }

        [Fact]
        public async Task Post_AddsNewWarehouse_WhenAuthorized()
        {
            // Arrange
            var newWarehouse = new WarehouseModel
            {
                code = "WH003",
                name = "New Warehouse",
                address = "789 New St",
                zip = "13579",
                city = "New City",
                province = "New Province",
                country = "Test Country",
                contact = new Contact
                {
                    name = "Bob Wilson",
                    phone = "555-555-5555",
                    email = "bob@warehouse.com"
                },
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act
            var result = await _controller.Post(newWarehouse) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdWarehouse = result.Value as WarehouseModel;
            createdWarehouse.Should().NotBeNull();
            createdWarehouse.code.Should().Be("WH003");
            createdWarehouse.name.Should().Be("New Warehouse");
        }

        [Fact]
        public async Task Post_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var newWarehouse = new WarehouseModel
            {
                code = "WH003",
                name = "New Warehouse"
            };

            // Act
            var result = await _controller.Post(newWarehouse);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }

        [Fact]
        public async Task Put_UpdatesExistingWarehouse_WhenAuthorized()
        {
            // Arrange
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull();

            var updateWarehouse = new WarehouseModel
            {
                code = "WH001-UPD",
                name = "Updated Warehouse",
                address = "Updated Address",
                contact = new Contact
                {
                    name = "Updated Contact",
                    email = "updated@warehouse.com"
                }
            };

            // Act
            var result = await _controller.Put(warehouse.id, updateWarehouse) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Warehouse has been updated");

            var updatedWarehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.id == warehouse.id);
            updatedWarehouse.Should().NotBeNull();
            updatedWarehouse.code.Should().Be("WH001-UPD");
            updatedWarehouse.name.Should().Be("Updated Warehouse");
        }

        [Fact]
        public async Task Put_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull();

            var updateWarehouse = new WarehouseModel
            {
                code = "WH001-UPD",
                name = "Updated Warehouse"
            };

            // Act
            var result = await _controller.Put(warehouse.id, updateWarehouse);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }

        [Fact]
        public async Task Delete_RemovesWarehouse_WhenAuthorized()
        {
            // Arrange
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(warehouse.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Warehouse has been deleted");

            var deletedWarehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.id == warehouse.id);
            deletedWarehouse.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(warehouse.id);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }
    }
}