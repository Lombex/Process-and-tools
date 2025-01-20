using CSharpAPI.Controller;
using CSharpAPI.Models;
using CSharpAPI.Service;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Integration.Tests.Tests
{
    public class SuppliersControllerTest : IntegrationTestBase
    {
        private readonly SupplierController _controller;
        private readonly ISupplierService _service;

        public SuppliersControllerTest()
        {
            _service = new SupplierService(DbContext);
            _controller = new SupplierController(_service);

            SeedTestData();
        }

        private async void SeedTestData()
        {
            var suppliers = new List<SupplierModel>
            {
                new SupplierModel
                {
                    id = 1,
                    name = "Supplier 1",
                    code = "S001",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new SupplierModel
                {
                    id = 2,
                    name = "Supplier 2",
                    code = "S002",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.Suppliers.AddRangeAsync(suppliers);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_ReturnsAllSuppliers()
        {
            var result = await _controller.GetAllSuppliers() as OkObjectResult;
            var suppliers = result?.Value as List<SupplierModel>;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            suppliers.Should().HaveCount(2);
            suppliers.Should().Contain(s => s.name == "Supplier 1");
            suppliers.Should().Contain(s => s.name == "Supplier 2");
        }

        [Fact]
        public async Task GetById_ReturnsSupplier()
        {
            var result = await _controller.GetSupplierById(1) as OkObjectResult;
            var supplier = result?.Value as SupplierModel;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            supplier.Should().NotBeNull();
            supplier.name.Should().Be("Supplier 1");
            supplier.code.Should().Be("S001");
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_ForInvalidId()
        {
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.GetSupplierById(999);
            });

            exception.Message.Should().Be("Supplier not found!");
        }

        [Fact]
        public async Task Create_AddsNewSupplier()
        {
            var newSupplier = new SupplierModel
            {
                name = "Supplier 3",
                code = "S003"
            };

            var result = await _controller.CreateSupplier(newSupplier) as CreatedAtActionResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdSupplier = result.Value as SupplierModel;
            createdSupplier.Should().NotBeNull();
            createdSupplier.name.Should().Be("Supplier 3");
            createdSupplier.code.Should().Be("S003");
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelIsNull()
        {
            var result = await _controller.CreateSupplier(null) as BadRequestObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
            result.Value.Should().Be("Request is empty!");
        }

        [Fact]
        public async Task Update_UpdatesExistingSupplier()
        {
            var updatedSupplier = new SupplierModel
            {
                name = "Updated Supplier",
                code = "S001-UPD"
            };

            var result = await _controller.UpdateSupplier(1, updatedSupplier) as OkObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Supplier Updated Supplier has been updated!");
        }

        [Fact]
        public async Task Update_ReturnsNotFound_ForInvalidId()
        {
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                var updatedSupplier = new SupplierModel
                {
                    name = "Nonexistent Supplier",
                    code = "S999"
                };
                await _controller.UpdateSupplier(999, updatedSupplier);
            });

            exception.Message.Should().Be("Supplier not found!");
        }

        [Fact]
        public async Task Delete_RemovesSupplier()
        {
            var result = await _controller.DeleteSupplier(1) as OkObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Supplier has been deleted.");

            var suppliers = await _service.GetAllSuppliers();
            suppliers.Should().NotContain(s => s.id == 1);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_ForInvalidId()
        {
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.DeleteSupplier(999);
            });

            exception.Message.Should().Be("Supplier not found!");
        }
    }
}
