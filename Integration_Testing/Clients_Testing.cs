using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Services;
using CSharpAPI.Services.Auth;
using CSharpAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CSharpAPI.Models.Auth;

namespace Integration_Testing
{
    public class Clients_Testing
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed initial data
            context.ClientModels.AddRange(
                new ClientModel
                {
                    id = 1,
                    name = "Client A",
                    address = "123 Main St",
                    city = "Metropolis",
                    zip_code = "12345",
                    province = "State A",
                    country = "Country A",
                    contact_name = "John Doe",
                    contact_phone = "1234567890",
                    contact_email = "john.doe@example.com",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new ClientModel
                {
                    id = 2,
                    name = "Client B",
                    address = "456 Elm St",
                    city = "Gotham",
                    zip_code = "67890",
                    province = "State B",
                    country = "Country B",
                    contact_name = "Jane Smith",
                    contact_phone = "0987654321",
                    contact_email = "jane.smith@example.com",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                }
            );

            context.SaveChanges();
            return context;
        }

        private Mock<IAuthService> GetMockAuthService()
        {
            var mockAuthService = new Mock<IAuthService>();
            mockAuthService.Setup(auth => auth.HasAccess(It.IsAny<ApiUser>(), "clients", It.IsAny<string>())).ReturnsAsync(true);
            return mockAuthService;
        }

        [Fact]
        public async Task GetAllClients_ReturnsAllClients()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var authService = GetMockAuthService();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, authService.Object);

            var result = await controller.GetAllClients() as OkObjectResult;
            var clients = result?.Value as List<ClientModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, clients.Count);
        }

        [Fact]
        public async Task GetClientById_ReturnsClient()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var authService = GetMockAuthService();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, authService.Object);

            var result = await controller.GetClientById(1) as OkObjectResult;
            var client = result?.Value as ClientModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(client);
            Xunit.Assert.Equal("Client A", client.name);
        }

        [Fact]
        public async Task GetClientById_ReturnsNotFoundForInvalidId()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var authService = GetMockAuthService();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, authService.Object);

            var result = await controller.GetClientById(99) as NotFoundObjectResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(404, result.StatusCode);
            Xunit.Assert.Equal("Client with id 99 not found.", result.Value);
        }

        [Fact]
        public async Task CreateClient_AddsNewClient()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var authService = GetMockAuthService();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, authService.Object);

            var newClient = new ClientModel
            {
                id = 3,
                name = "Client C",
                address = "789 Pine St",
                city = "Star City",
                zip_code = "11223",
                province = "State C",
                country = "Country C",
                contact_name = "Clark Kent",
                contact_phone = "5551234567",
                contact_email = "clark.kent@example.com",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var result = await controller.CreateClient(newClient) as CreatedAtActionResult;
            var clients = await dbContext.ClientModels.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(3, clients.Count);
            Xunit.Assert.Contains(clients, c => c.name == "Client C");
        }

        [Fact]
        public async Task UpdateClient_UpdatesExistingClient()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var authService = GetMockAuthService();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, authService.Object);

            var updatedClient = new ClientModel
            {
                name = "Client A Updated",
                address = "123 Main St Updated",
                city = "Metropolis Updated",
                zip_code = "54321",
                province = "State Z",
                country = "Country Z",
                contact_name = "John Updated",
                contact_phone = "5559876543",
                contact_email = "john.updated@example.com",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var result = await controller.UpdateClient(1, updatedClient) as NoContentResult;
            var client = await dbContext.ClientModels.FirstOrDefaultAsync(c => c.id == 1);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(204, result.StatusCode);
            Xunit.Assert.NotNull(client);
            Xunit.Assert.Equal("Client A Updated", client.name);
        }

        [Fact]
        public async Task DeleteClient_RemovesClient()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var authService = GetMockAuthService();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, authService.Object);

            var result = await controller.DeleteClient(1) as NoContentResult;
            var clients = await dbContext.ClientModels.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(204, result.StatusCode);
            Xunit.Assert.Single(clients);
            Xunit.Assert.DoesNotContain(clients, c => c.id == 1);
        }

        [Fact]
        public async Task ClientOrders_ReturnsOrdersForClient()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var authService = GetMockAuthService();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, authService.Object);

            var result = await controller.ClientOrders(1) as OkObjectResult;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
        }
    }
}
