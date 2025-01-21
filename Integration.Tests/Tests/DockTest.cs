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
    public class DockTest : IntegrationTestBase
    {
        private readonly DockController _controller;
        private readonly IDockService _service;
        private readonly IAuthService _authService;

        public DockTest()
        {
            _service = new DockService(DbContext);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new DockController(_service, _authService);

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

            // Seed test dock data
            var docks = new List<DockModel>
            {
                new DockModel
                {
                    warehouse_id = warehouse1.id,
                    code = "DOCK001",
                    name = "Dock A",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new DockModel
                {
                    warehouse_id = warehouse1.id,
                    code = "DOCK002",
                    name = "Dock B",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new DockModel
                {
                    warehouse_id = warehouse2.id,
                    code = "DOCK003",
                    name = "Dock C",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            foreach (var dock in docks)
            {
                await _service.AddDock(dock);
            }
        }

        [Fact]
        public async Task GetAll_ReturnsAllDocks_WhenAuthorized()
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
            var docksProperty = responseType.GetProperty("Docks").GetValue(response) as IEnumerable<object>;

            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(3);
            totalPagesProperty.Should().Be(1);

            docksProperty.Should().NotBeNull();
            var docks = docksProperty.ToList();
            docks.Should().HaveCount(3);

            // Check specific docks
            var firstDock = docks.First();
            var firstDockType = firstDock.GetType();
            (firstDockType.GetProperty("Code").GetValue(firstDock) as string).Should().Be("DOCK001");
            
            var lastDock = docks.Last();
            var lastDockType = lastDock.GetType();
            (lastDockType.GetProperty("Code").GetValue(lastDock) as string).Should().Be("DOCK003");
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
        public async Task Get_ReturnsDock_WhenAuthorized()
        {
            // Arrange
            var docks = await _service.GetAllDocks();
            var dock = docks.FirstOrDefault(d => d.code == "DOCK001");
            dock.Should().NotBeNull();

            // Act
            var result = await _controller.Get(dock.id.Value) as OkObjectResult;
            var returnedDock = result?.Value as DockModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedDock.Should().NotBeNull();
            returnedDock.code.Should().Be("DOCK001");
            returnedDock.name.Should().Be("Dock A");
        }

        [Fact]
        public async Task Get_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var docks = await _service.GetAllDocks();
            var dock = docks.FirstOrDefault(d => d.code == "DOCK001");
            dock.Should().NotBeNull();

            // Act
            var result = await _controller.Get(dock.id.Value);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }

        [Fact]
        public async Task Post_AddsNewDock_WhenAuthorized()
        {
            // Arrange
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull();

            var newDock = new DockModel
            {
                warehouse_id = warehouse.id,
                code = "DOCK004",
                name = "Dock D",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act
            var result = await _controller.Post(newDock) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdDock = result.Value as DockModel;
            createdDock.Should().NotBeNull();
            createdDock.code.Should().Be("DOCK004");
            createdDock.name.Should().Be("Dock D");
        }

        [Fact]
        public async Task Post_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull();

            var newDock = new DockModel
            {
                warehouse_id = warehouse.id,
                code = "DOCK004",
                name = "Dock D"
            };

            // Act
            var result = await _controller.Post(newDock);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }

        [Fact]
        public async Task Put_UpdatesExistingDock_WhenAuthorized()
        {
            // Arrange
            var docks = await _service.GetAllDocks();
            var dock = docks.FirstOrDefault(d => d.code == "DOCK001");
            dock.Should().NotBeNull();

            var updateDock = new DockModel
            {
                warehouse_id = dock.warehouse_id,
                code = "DOCK001-UPD",
                name = "Updated Dock A"
            };

            // Act
            var result = await _controller.Put(dock.id.Value, updateDock) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Dock has been updated");

            var updatedDock = await _service.GetDockById(dock.id.Value);
            updatedDock.Should().NotBeNull();
            updatedDock.code.Should().Be("DOCK001-UPD");
            updatedDock.name.Should().Be("Updated Dock A");
        }

        [Fact]
        public async Task Put_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var docks = await _service.GetAllDocks();
            var dock = docks.FirstOrDefault(d => d.code == "DOCK001");
            dock.Should().NotBeNull();

            var updateDock = new DockModel
            {
                code = "DOCK001-UPD",
                name = "Updated Dock A"
            };

            // Act
            var result = await _controller.Put(dock.id.Value, updateDock);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }

        [Fact]
        public async Task Delete_RemovesDock_WhenAuthorized()
        {
            // Arrange
            var docks = await _service.GetAllDocks();
            var dock = docks.FirstOrDefault(d => d.code == "DOCK001");
            dock.Should().NotBeNull();
        
            // Act
            var result = await _controller.Delete(dock.id.Value) as OkObjectResult;
        
            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Dock has been deleted");
        
            // Check that the dock no longer exists
            var allDocksAfterDeletion = await _service.GetAllDocks();
            allDocksAfterDeletion.Should().NotContain(d => d.id == dock.id);
        }

        [Fact]
        public async Task Delete_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var docks = await _service.GetAllDocks();
            var dock = docks.FirstOrDefault(d => d.code == "DOCK001");
            dock.Should().NotBeNull();

            // Act
            var result = await _controller.Delete(dock.id.Value);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }

        [Fact]
        public async Task GetByWarehouseId_ReturnsDock_WhenAuthorized()
        {
            // Arrange
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull("Test warehouse should exist");

            // Act
            var actionResult = await _controller.GetByWarehouseId(warehouse.id);

            // Assert
            actionResult.Should().NotBeNull("Action result should not be null");
            actionResult.Should().BeOfType<OkObjectResult>("Result should be OkObjectResult");

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull("OkObjectResult should not be null");

            var returnedDocks = okResult.Value as IEnumerable<DockModel>;
            returnedDocks.Should().NotBeNull("Returned docks should not be null");

            returnedDocks.Should().HaveCountGreaterThan(0, "At least one dock should be returned");
            returnedDocks.Should().Contain(d => d.code == "DOCK001", "Should contain DOCK001");
            returnedDocks.Should().Contain(d => d.code == "DOCK002", "Should contain DOCK002");
        }
        [Fact]
        public async Task GetByWarehouseId_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var warehouse = await DbContext.Warehouse.FirstOrDefaultAsync(w => w.code == "WH001");
            warehouse.Should().NotBeNull();

            // Act
            var result = await _controller.GetByWarehouseId(warehouse.id);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(403);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Access denied" });
        }
    }
}