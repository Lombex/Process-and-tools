using CSharpAPI.Controller;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Services.Auth;
using CSharpAPI.Data;
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
    public class SupplierTest : IntegrationTestBase
    {
        private readonly SupplierController _controller;
        private readonly ISupplierService _service;
        private readonly IAuthService _authService;

        public SupplierTest()
        {
            _service = new SupplierService(DbContext);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new SupplierController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.Suppliers.RemoveRange(DbContext.Suppliers);
            DbContext.itemModels.RemoveRange(DbContext.itemModels);
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var suppliers = new List<SupplierModel>
            {
                new SupplierModel
                {
                    code = "SUP001",
                    name = "Supplier 1",
                    address = "123 Main St",
                    city = "Test City",
                    contact = new Contact
                    {
                        name = "John Contact",
                        phone = "123-456-7890",
                        email = "john@supplier1.com"
                    },
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new SupplierModel
                {
                    code = "SUP002",
                    name = "Supplier 2",
                    address = "456 Second St",
                    city = "Test City",
                    contact = new Contact
                    {
                        name = "Jane Contact",
                        phone = "098-765-4321",
                        email = "jane@supplier2.com"
                    },
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.Suppliers.AddRangeAsync(suppliers);
            await DbContext.SaveChangesAsync();

            var supplier1 = await DbContext.Suppliers.FirstOrDefaultAsync(s => s.code == "SUP001");
            if (supplier1 == null) throw new Exception("Failed to seed supplier data");

            var items = new List<ItemModel>
            {
                new ItemModel
                {
                    uid = "P000001",
                    code = "ITEM001",
                    description = "Test Item 1",
                    supplier_id = supplier1.id,
                    supplier_code = "SUP001",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ItemModel
                {
                    uid = "P000002",
                    code = "ITEM002",
                    description = "Test Item 2",
                    supplier_id = supplier1.id,
                    supplier_code = "SUP001",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.itemModels.AddRangeAsync(items);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllSuppliers_ReturnsAllSuppliers_WhenAuthorized()
        {
            // Act
            var actionResult = await _controller.GetAllSuppliers(0);
            
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
            var suppliersProperty = responseType.GetProperty("Suppliers").GetValue(response) as IEnumerable<object>;

            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(2);
            totalPagesProperty.Should().Be(1);

            suppliersProperty.Should().NotBeNull();
            var suppliers = suppliersProperty.ToList();
            suppliers.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllSuppliers_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            // Act
            var result = await _controller.GetAllSuppliers(0);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetSupplierById_ReturnsSupplier_WhenAuthorized()
        {
            // Arrange
            var supplier = await DbContext.Suppliers.FirstOrDefaultAsync(s => s.code == "SUP001");
            supplier.Should().NotBeNull();

            // Act
            var result = await _controller.GetSupplierById(supplier.id) as OkObjectResult;
            var returnedSupplier = result?.Value as SupplierModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedSupplier.Should().NotBeNull();
            returnedSupplier.code.Should().Be("SUP001");
            returnedSupplier.name.Should().Be("Supplier 1");
            returnedSupplier.contact.Should().NotBeNull();
            returnedSupplier.contact.name.Should().Be("John Contact");
        }

        [Fact]
        public async Task GetSupplierById_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var supplier = await DbContext.Suppliers.FirstOrDefaultAsync(s => s.code == "SUP001");
            supplier.Should().NotBeNull();

            // Act
            var result = await _controller.GetSupplierById(supplier.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetItemFromSupplierId_ReturnsItems_WhenAuthorized()
        {
            // Arrange
            var supplier = await DbContext.Suppliers.FirstOrDefaultAsync(s => s.code == "SUP001");
            supplier.Should().NotBeNull();

            // Act
            var result = await _controller.GetItemFromSupplierId(supplier.id) as OkObjectResult;
            var items = result?.Value as List<ItemModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            items.Should().NotBeNull();
            items.Should().HaveCount(2);
            items.Should().OnlyContain(i => i.supplier_id == supplier.id);
        }

        [Fact]
        public async Task GetItemFromSupplierId_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var supplier = await DbContext.Suppliers.FirstOrDefaultAsync(s => s.code == "SUP001");
            supplier.Should().NotBeNull();

            // Act
            var result = await _controller.GetItemFromSupplierId(supplier.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task CreateSupplier_AddsNewSupplier_WhenAuthorized()
        {
            // Arrange
            var newSupplier = new SupplierModel
            {
                code = "SUP003",
                name = "Supplier 3",
                address = "789 Third St",
                city = "Test City",
                contact = new Contact
                {
                    name = "Bob Contact",
                    phone = "111-222-3333",
                    email = "bob@supplier3.com"
                },
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act
            var result = await _controller.CreateSupplier(newSupplier) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdSupplier = result.Value as SupplierModel;
            createdSupplier.Should().NotBeNull();
            createdSupplier.code.Should().Be("SUP003");
            createdSupplier.name.Should().Be("Supplier 3");
        }

        [Fact]
        public async Task CreateSupplier_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var newSupplier = new SupplierModel
            {
                code = "SUP003",
                name = "Supplier 3"
            };

            // Act
            var result = await _controller.CreateSupplier(newSupplier);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task UpdateSupplier_UpdatesExistingSupplier_WhenAuthorized()
        {
            // Arrange
            var supplier = await DbContext.Suppliers.FirstOrDefaultAsync(s => s.code == "SUP001");
            supplier.Should().NotBeNull();

            var updateSupplier = new SupplierModel
            {
                code = "SUP001-UPD",
                name = "Updated Supplier",
                address = "Updated Address",
                contact = new Contact
                {
                    name = "Updated Contact",
                    email = "updated@supplier.com"
                }
            };

            // Act
            var result = await _controller.UpdateSupplier(supplier.id, updateSupplier) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be($"Supplier {updateSupplier.name} has been updated!");

            var updatedSupplier = await _service.GetSupplierById(supplier.id);
            updatedSupplier.code.Should().Be("SUP001-UPD");
            updatedSupplier.name.Should().Be("Updated Supplier");
        }

        [Fact]
        public async Task UpdateSupplier_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var supplier = await DbContext.Suppliers.FirstOrDefaultAsync(s => s.code == "SUP001");
            supplier.Should().NotBeNull();

            var updateSupplier = new SupplierModel
            {
                code = "SUP001-UPD",
                name = "Updated Supplier"
            };

            // Act
            var result = await _controller.UpdateSupplier(supplier.id, updateSupplier);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task DeleteSupplier_RemovesSupplier_WhenAuthorized()
        {
            // Arrange
            var supplier = await DbContext.Suppliers.FirstOrDefaultAsync(s => s.code == "SUP001");
            supplier.Should().NotBeNull();

            // Act
            var result = await _controller.DeleteSupplier(supplier.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Supplier has been deleted.");

            var deletedSupplier = await DbContext.Suppliers.FirstOrDefaultAsync(s => s.id == supplier.id);
            deletedSupplier.Should().BeNull();
        }

        [Fact]
        public async Task DeleteSupplier_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var supplier = await DbContext.Suppliers.FirstOrDefaultAsync(s => s.code == "SUP001");
            supplier.Should().NotBeNull();

            // Act
            var result = await _controller.DeleteSupplier(supplier.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }
    }
}
