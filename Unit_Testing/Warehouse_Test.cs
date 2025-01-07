using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class WarehouseServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public WarehouseServiceTests()
        {
            // Set up In-Memory database options
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "WarehouseTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task GetAllWarehouses_ReturnsListOfWarehouses()
        {
            // Arrange: Create mock data for warehouses
            var warehouseList = new List<WarehouseModel>
            {
                new WarehouseModel 
                { 
                    id = 1, 
                    code = "WH001", 
                    name = "Warehouse 1", 
                    address = "123 Street", 
                    zip = "12345", 
                    city = "City A", 
                    province = "Province A", 
                    country = "Country A", 
                    contact = new Contact 
                    { 
                        name = "John Doe", 
                        phone = "1234567890", 
                        email = "john.doe@example.com" 
                    },
                    created_at = DateTime.Now, 
                    updated_at = DateTime.Now 
                },
                new WarehouseModel 
                { 
                    id = 2, 
                    code = "WH002", 
                    name = "Warehouse 2", 
                    address = "456 Avenue", 
                    zip = "67890", 
                    city = "City B", 
                    province = "Province B", 
                    country = "Country B", 
                    contact = new Contact 
                    { 
                        name = "Jane Doe", 
                        phone = "0987654321", 
                        email = "jane.doe@example.com" 
                    },
                    created_at = DateTime.Now, 
                    updated_at = DateTime.Now 
                }
            };

            using (var context = CreateDbContext())
            {
                context.Warehouse.AddRange(warehouseList);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve all warehouses
            List<WarehouseModel> result;
            using (var context = CreateDbContext())
            {
                var service = new WarehouseService(context);
                result = await service.GetAllWarehouses();
            }

            // Assert: Verify that the correct number of warehouses is returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetWarehouseById_ValidId_ReturnsWarehouse()
        {
            // Arrange: Add a warehouse to the database
            var warehouse = new WarehouseModel 
            { 
                id = 1, 
                code = "WH001", 
                name = "Warehouse 1", 
                address = "123 Street", 
                zip = "12345", 
                city = "City A", 
                province = "Province A", 
                country = "Country A", 
                contact = new Contact 
                { 
                    name = "John Doe", 
                    phone = "1234567890", 
                    email = "john.doe@example.com" 
                },
                created_at = DateTime.Now, 
                updated_at = DateTime.Now 
            };

            using (var context = CreateDbContext())
            {
                context.Warehouse.Add(warehouse);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve the warehouse by ID
            WarehouseModel result;
            using (var context = CreateDbContext())
            {
                var service = new WarehouseService(context);
                result = await service.GetWarehouseById(1);
            }

            // Assert: Verify that the warehouse was retrieved correctly
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Warehouse 1", result.name);
        }

        [Fact]
        public async Task AddWarehouse_ValidWarehouse_CreatesWarehouse()
        {
            // Arrange: Create a new warehouse
            var newWarehouse = new WarehouseModel
            {
                code = "WH003",
                name = "Warehouse 3",
                address = "789 Boulevard",
                zip = "12367",
                city = "City C",
                province = "Province C",
                country = "Country C",
                contact = new Contact
                {
                    name = "Alice Johnson",
                    phone = "1122334455",
                    email = "alice.johnson@example.com"
                },
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            // Act: Add the new warehouse to the database
            using (var context = CreateDbContext())
            {
                var service = new WarehouseService(context);
                await service.AddWarehouse(newWarehouse);
            }

            // Assert: Verify that the warehouse is added to the database
            using (var context = CreateDbContext())
            {
                var result = await context.Warehouse.FirstOrDefaultAsync(w => w.code == "WH003");
                Assert.NotNull(result);
                Assert.Equal("Warehouse 3", result.name);
            }
        }

        [Fact]
        public async Task UpdateWarehouse_ValidWarehouse_UpdatesWarehouse()
        {
            // Arrange: Add a warehouse first
            var warehouse = new WarehouseModel
            {
                id = 1,
                code = "WH001",
                name = "Warehouse 1",
                address = "123 Street",
                zip = "12345",
                city = "City A",
                province = "Province A",
                country = "Country A",
                contact = new Contact
                {
                    name = "John Doe",
                    phone = "1234567890",
                    email = "john.doe@example.com"
                },
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            using (var context = CreateDbContext())
            {
                context.Warehouse.Add(warehouse);
                await context.SaveChangesAsync();
            }

            var updatedWarehouse = new WarehouseModel
            {
                code = "WH001",
                name = "Updated Warehouse 1",
                address = "123 Updated Street",
                zip = "54321",
                city = "Updated City",
                province = "Updated Province",
                country = "Updated Country",
                contact = new Contact
                {
                    name = "Updated John Doe",
                    phone = "0987654321",
                    email = "updated.john.doe@example.com"
                },
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            // Act: Update the warehouse
            using (var context = CreateDbContext())
            {
                var service = new WarehouseService(context);
                await service.UpdateWarehouse(1, updatedWarehouse);
            }

            // Assert: Verify that the warehouse was updated correctly
            using (var context = CreateDbContext())
            {
                var result = await context.Warehouse.FirstOrDefaultAsync(w => w.id == 1);
                Assert.NotNull(result);
                Assert.Equal("Updated Warehouse 1", result.name);
                Assert.Equal("123 Updated Street", result.address);
            }
        }

        [Fact]
        public async Task DeleteWarehouse_ValidWarehouse_DeletesWarehouse()
        {
            // Arrange: Add a warehouse first
            var warehouse = new WarehouseModel
            {
                id = 1,
                code = "WH001",
                name = "Warehouse 1",
                address = "123 Street",
                zip = "12345",
                city = "City A",
                province = "Province A",
                country = "Country A",
                contact = new Contact
                {
                    name = "John Doe",
                    phone = "1234567890",
                    email = "john.doe@example.com"
                },
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            using (var context = CreateDbContext())
            {
                context.Warehouse.Add(warehouse);
                await context.SaveChangesAsync();
            }

            // Act: Delete the warehouse
            using (var context = CreateDbContext())
            {
                var service = new WarehouseService(context);
                await service.DeleteWarehouse(1);
            }

            // Assert: Verify that the warehouse was deleted from the database
            using (var context = CreateDbContext())
            {
                var result = await context.Warehouse.FirstOrDefaultAsync(w => w.id == 1);
                Assert.Null(result);
            }
        }
    }
}
