using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class SupplierServiceUnitTests
    {
        [Fact]
        public async Task GetAllSuppliers_ReturnsListOfSuppliers()
        {
            // Arrange
            var mockSuppliers = new List<SupplierModel>
            {
                new SupplierModel
                {
                    id = 1,
                    code = "SUP001",
                    name = "Supplier 1",
                    address = "123 Street",
                    city = "City A"
                },
                new SupplierModel
                {
                    id = 2,
                    code = "SUP002",
                    name = "Supplier 2",
                    address = "456 Avenue",
                    city = "City B"
                }
            };

            var mockSupplierService = new Mock<ISupplierService>();
            mockSupplierService
                .Setup(service => service.GetAllSuppliers())
                .ReturnsAsync(mockSuppliers);

            // Act
            var result = await mockSupplierService.Object.GetAllSuppliers();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Supplier 1", result[0].name);
            Assert.Equal("Supplier 2", result[1].name);
        }

        [Fact]
        public async Task GetSupplierById_ValidId_ReturnsSupplier()
        {
            // Arrange
            var supplier = new SupplierModel
            {
                id = 1,
                code = "SUP001",
                name = "Supplier 1",
                address = "123 Street",
                city = "City A"
            };

            var mockSupplierService = new Mock<ISupplierService>();
            mockSupplierService
                .Setup(service => service.GetSupplierById(1))
                .ReturnsAsync(supplier);

            // Act
            var result = await mockSupplierService.Object.GetSupplierById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Supplier 1", result.name);
        }

        [Fact]
        public async Task CreateSupplier_ValidSupplier_CallsCreateMethod()
        {
            // Arrange
            var newSupplier = new SupplierModel
            {
                code = "SUP003",
                name = "Supplier 3",
                address = "789 Boulevard",
                city = "City C"
            };

            var mockSupplierService = new Mock<ISupplierService>();
            mockSupplierService
                .Setup(service => service.CreateSupplier(It.IsAny<SupplierModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockSupplierService.Object.CreateSupplier(newSupplier);

            // Assert
            mockSupplierService.Verify(service => service.CreateSupplier(It.Is<SupplierModel>(s => s.code == "SUP003" && s.name == "Supplier 3")), Times.Once);
        }

        [Fact]
        public async Task UpdateSupplier_ValidSupplier_CallsUpdateMethod()
        {
            // Arrange
            var updatedSupplier = new SupplierModel
            {
                id = 1,
                code = "SUP001",
                name = "Updated Supplier 1",
                address = "123 Updated Street",
                city = "Updated City"
            };

            var mockSupplierService = new Mock<ISupplierService>();
            mockSupplierService
                .Setup(service => service.UpdateSupplier(It.IsAny<int>(), It.IsAny<SupplierModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockSupplierService.Object.UpdateSupplier(1, updatedSupplier);

            // Assert
            mockSupplierService.Verify(service => service.UpdateSupplier(1, It.Is<SupplierModel>(s => s.name == "Updated Supplier 1" && s.address == "123 Updated Street")), Times.Once);
        }

        [Fact]
        public async Task DeleteSupplier_ValidId_CallsDeleteMethod()
        {
            // Arrange
            var mockSupplierService = new Mock<ISupplierService>();
            mockSupplierService
                .Setup(service => service.DeleteSupplier(1))
                .Returns(Task.CompletedTask);

            // Act
            await mockSupplierService.Object.DeleteSupplier(1);

            // Assert
            mockSupplierService.Verify(service => service.DeleteSupplier(1), Times.Once);
        }
    }
}
