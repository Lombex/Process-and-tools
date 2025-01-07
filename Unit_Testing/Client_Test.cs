using Xunit;
using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class ClientsServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public ClientsServiceTests()
        {
            // Set up In-Memory database options
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "ClientsTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task GetAllClients_ReturnsListOfClients()
        {
            // Arrange: Create mock data for Clients
            var clientList = new List<ClientModel>
            {
                new ClientModel
                {
                    id = 1,
                    name = "Client A",
                    address = "Address 1",
                    city = "City A",
                    zip_code = "12345",
                    province = "Province A",
                    country = "Country A",
                    contact_name = "Contact A",
                    contact_phone = "123-456-7890",
                    contact_email = "clienta@example.com",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ClientModel
                {
                    id = 2,
                    name = "Client B",
                    address = "Address 2",
                    city = "City B",
                    zip_code = "67890",
                    province = "Province B",
                    country = "Country B",
                    contact_name = "Contact B",
                    contact_phone = "987-654-3210",
                    contact_email = "clientb@example.com",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            using (var context = CreateDbContext())
            {
                context.ClientModels.AddRange(clientList);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve all clients
            List<ClientModel> result;
            using (var context = CreateDbContext())
            {
                var service = new ClientsService(context);
                result = await service.GetAllClients();
            }

            // Assert: Verify that the correct number of clients is returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetClientById_ValidId_ReturnsClient()
        {
            // Arrange: Add a Client to the database
            var client = new ClientModel
            {
                id = 1,
                name = "Client A",
                address = "Address 1",
                city = "City A",
                zip_code = "12345",
                province = "Province A",
                country = "Country A",
                contact_name = "Contact A",
                contact_phone = "123-456-7890",
                contact_email = "clienta@example.com",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.ClientModels.Add(client);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve the client by ID
            ClientModel result;
            using (var context = CreateDbContext())
            {
                var service = new ClientsService(context);
                result = await service.GetClientById(1);
            }

            // Assert: Verify that the client was retrieved correctly
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Client A", result.name);
        }

        [Fact]
        public async Task AddClient_ValidClient_CreatesClient()
        {
            // Arrange: Create a new Client
            var newClient = new ClientModel
            {
                name = "Client C",
                address = "Address 3",
                city = "City C",
                zip_code = "54321",
                province = "Province C",
                country = "Country C",
                contact_name = "Contact C",
                contact_phone = "111-222-3333",
                contact_email = "clientc@example.com",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act: Add the new Client to the database
            using (var context = CreateDbContext())
            {
                var service = new ClientsService(context);
                await service.AddClient(newClient);
            }

            // Assert: Verify that the client is added to the database
            using (var context = CreateDbContext())
            {
                var result = await context.ClientModels.FirstOrDefaultAsync(c => c.name == "Client C");
                Assert.NotNull(result);
                Assert.Equal("Client C", result.name);
            }
        }

        [Fact]
        public async Task UpdateClient_ValidClient_UpdatesClient()
        {
            // Arrange: Add a Client to the database
            var client = new ClientModel
            {
                id = 1,
                name = "Client A",
                address = "Address 1",
                city = "City A",
                zip_code = "12345",
                province = "Province A",
                country = "Country A",
                contact_name = "Contact A",
                contact_phone = "123-456-7890",
                contact_email = "clienta@example.com",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.ClientModels.Add(client);
                await context.SaveChangesAsync();
            }

            // Act: Update the Client
            var updatedClient = new ClientModel
            {
                name = "Updated Client A",
                address = "Updated Address 1",
                city = "Updated City A",
                zip_code = "54321",
                province = "Updated Province A",
                country = "Updated Country A",
                contact_name = "Updated Contact A",
                contact_phone = "987-654-3210",
                contact_email = "updatedclienta@example.com",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                var service = new ClientsService(context);
                await service.UpdateClient(1, updatedClient);
            }

            // Assert: Verify that the client was updated correctly
            using (var context = CreateDbContext())
            {
                var result = await context.ClientModels.FirstOrDefaultAsync(c => c.id == 1);
                Assert.NotNull(result);
                Assert.Equal("Updated Client A", result.name);
            }
        }

        [Fact]
        public async Task DeleteClient_ValidClient_DeletesClient()
        {
            // Arrange: Add a Client to the database
            var client = new ClientModel
            {
                id = 1,
                name = "Client A",
                address = "Address 1",
                city = "City A",
                zip_code = "12345",
                province = "Province A",
                country = "Country A",
                contact_name = "Contact A",
                contact_phone = "123-456-7890",
                contact_email = "clienta@example.com",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.ClientModels.Add(client);
                await context.SaveChangesAsync();
            }

            // Act: Delete the Client
            using (var context = CreateDbContext())
            {
                var service = new ClientsService(context);
                await service.DeleteClient(1);
            }

            // Assert: Verify that the Client was deleted
            using (var context = CreateDbContext())
            {
                var result = await context.ClientModels.FirstOrDefaultAsync(c => c.id == 1);
                Assert.Null(result); // The client should be deleted
            }
        }

        [Fact]
        public async Task GetClientOrders_ValidClientId_ReturnsOrders()
        {
            var client = new ClientModel
            {
                id = 1,
                name = "Client A",
                address = "Address 1",
                city = "City A",
                zip_code = "12345",
                province = "Province A",
                country = "Country A",
                contact_name = "Contact A",
                contact_phone = "123-456-7890",
                contact_email = "clienta@example.com",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            var orderList = new List<OrderModel>
            {
                new OrderModel
                {
                    id = 1,
                    bill_to = 1,
                    ship_to = 1,
                    total_amount = 100,  // decimal type
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new OrderModel
                {
                    id = 2,
                    bill_to = 1,
                    ship_to = 1,
                    total_amount = 200,  // decimal type
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            using (var context = CreateDbContext())
            {
                context.ClientModels.Add(client);
                context.Order.AddRange(orderList);
                await context.SaveChangesAsync();
            }

            List<OrderModel> result;
            using (var context = CreateDbContext())
            {
                var service = new ClientsService(context);
                result = await service.GetClientOrders(1);
            }

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // Example of casting decimal to float
            float totalAmount = (float)result[0].total_amount;  // Cast decimal to float
            Assert.Equal(100.00f, totalAmount);  // Use the 'f' suffix for float literals
        }
    }
}
