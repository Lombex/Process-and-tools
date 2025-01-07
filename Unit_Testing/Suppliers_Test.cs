using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class SupplierServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public SupplierServiceTests()
        {
            // Set up In-Memory database options
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "SupplierTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task GetAllSuppliers_ReturnsListOfSuppliers()
        {
            // Arrange
            var supplierList = new List<SupplierModel>
            {
                new SupplierModel
                {
                    id = 1,
                    code = "SUP001",
                    name = "Supplier 1",
                    address = "123 Street",
                    address_extra = "Suite 101",
                    city = "City A",
                    zip_code = "12345",
                    province = "Province A",
                    contact_name = "John Doe",
                    phonenumber = "1234567890",
                    reference = "REF001",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new SupplierModel
                {
                    id = 2,
                    code = "SUP002",
                    name = "Supplier 2",
                    address = "456 Avenue",
                    address_extra = "Suite 202",
                    city = "City B",
                    zip_code = "67890",
                    province = "Province B",
                    contact_name = "Jane Doe",
                    phonenumber = "0987654321",
                    reference = "REF002",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                }
            };

            using (var context = CreateDbContext())
            {
                context.Suppliers.AddRange(supplierList);
                await context.SaveChangesAsync();
            }

            // Act
            List<SupplierModel> result;
            using (var context = CreateDbContext())
            {
                var service = new SupplierService(context);
                result = await service.GetAllSuppliers();
            }

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
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
                address_extra = "Suite 101",
                city = "City A",
                zip_code = "12345",
                province = "Province A",
                contact_name = "John Doe",
                phonenumber = "1234567890",
                reference = "REF001",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            using (var context = CreateDbContext())
            {
                context.Suppliers.Add(supplier);
                await context.SaveChangesAsync();
            }

            // Act
            SupplierModel result;
            using (var context = CreateDbContext())
            {
                var service = new SupplierService(context);
                result = await service.GetSupplierById(1);
            }

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Supplier 1", result.name);
        }

        [Fact]
        public async Task CreateSupplier_ValidSupplier_CreatesSupplier()
        {
            // Arrange
            var newSupplier = new SupplierModel
            {
                code = "SUP003",
                name = "Supplier 3",
                address = "789 Boulevard",
                address_extra = "Suite 303",
                city = "City C",
                zip_code = "11223",
                province = "Province C",
                contact_name = "Alice Johnson",
                phonenumber = "1122334455",
                reference = "REF003",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            // Act
            using (var context = CreateDbContext())
            {
                var service = new SupplierService(context);
                await service.CreateSupplier(newSupplier);
            }

            // Assert
            using (var context = CreateDbContext())
            {
                var result = await context.Suppliers.FirstOrDefaultAsync(s => s.code == "SUP003");
                Assert.NotNull(result);
                Assert.Equal("Supplier 3", result.name);
            }
        }

        [Fact]
        public async Task UpdateSupplier_ValidSupplier_UpdatesSupplier()
        {
            // Arrange
            var supplier = new SupplierModel
            {
                id = 1,
                code = "SUP001",
                name = "Supplier 1",
                address = "123 Street",
                address_extra = "Suite 101",
                city = "City A",
                zip_code = "12345",
                province = "Province A",
                contact_name = "John Doe",
                phonenumber = "1234567890",
                reference = "REF001",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            using (var context = CreateDbContext())
            {
                context.Suppliers.Add(supplier);
                await context.SaveChangesAsync();
            }

            var updatedSupplier = new SupplierModel
            {
                code = "SUP001",
                name = "Updated Supplier 1",
                address = "123 Updated Street",
                address_extra = "Suite 202",
                city = "Updated City",
                zip_code = "54321",
                province = "Updated Province",
                contact_name = "Updated John Doe",
                phonenumber = "0987654321",
                reference = "REF001",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            // Act
            using (var context = CreateDbContext())
            {
                var service = new SupplierService(context);
                await service.UpdateSupplier(1, updatedSupplier);
            }

            // Assert
            using (var context = CreateDbContext())
            {
                var result = await context.Suppliers.FirstOrDefaultAsync(s => s.id == 1);
                Assert.NotNull(result);
                Assert.Equal("Updated Supplier 1", result.name);
                Assert.Equal("123 Updated Street", result.address);
            }
        }

        [Fact]
        public async Task DeleteSupplier_ValidSupplier_DeletesSupplier()
        {
            // Arrange
            var supplier = new SupplierModel
            {
                id = 1,
                code = "SUP001",
                name = "Supplier 1",
                address = "123 Street",
                address_extra = "Suite 101",
                city = "City A",
                zip_code = "12345",
                province = "Province A",
                contact_name = "John Doe",
                phonenumber = "1234567890",
                reference = "REF001",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            using (var context = CreateDbContext())
            {
                context.Suppliers.Add(supplier);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = CreateDbContext())
            {
                var service = new SupplierService(context);
                await service.DeleteSupplier(1);
            }

            // Assert
            using (var context = CreateDbContext())
            {
                var result = await context.Suppliers.FirstOrDefaultAsync(s => s.id == 1);
                Assert.Null(result); // The supplier should be deleted
            }
        }
    }
}
