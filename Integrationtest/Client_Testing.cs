using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Services;
using CSharpAPI.Controllers;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class ClientsControllerTests
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            context.ClientModels.AddRange(
                new ClientModel
                {
                    id = 1,
                    name = "Client One",
                    address = "123 Client St",
                    city = "ClientCity",
                    zip_code = "12345",
                    province = "ClientProvince",
                    country = "ClientCountry",
                    contact_name = "John Contact",
                    contact_phone = "123-456-7890",
                    contact_email = "john@client.com",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ClientModel
                {
                    id = 2,
                    name = "Client Two",
                    address = "456 Client Ave",
                    city = "ClientVille",
                    zip_code = "67890",
                    province = "ClientState",
                    country = "ClientNation",
                    contact_name = "Jane Contact",
                    contact_phone = "098-765-4321",
                    contact_email = "jane@client.com",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            );

            context.SaveChanges();
            return context;
        }

        private Mock<IAuthService> GetMockAuthService()
        {
            var mockAuthService = new Mock<IAuthService>();
            mockAuthService.Setup(x => x.HasAccess(It.IsAny<ApiUser>(), It.IsAny<string>(), It.IsAny<string>()))
                          .ReturnsAsync(true);
            return mockAuthService;
        }

        [Fact]
        public async Task GetAllClients_ReturnsAllClients()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, GetMockAuthService().Object);

            var result = await controller.GetAllClients() as OkObjectResult;
            var clients = result?.Value as IEnumerable<ClientModel>;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(2, clients.Count());
        }

        [Fact]
        public async Task GetClientById_ReturnsClient()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, GetMockAuthService().Object);

            var result = await controller.GetClientById(1) as OkObjectResult;
            var client = result?.Value as ClientModel;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Client One", client.name);
        }

        [Fact]
        public async Task CreateClient_AddsNewClient()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, GetMockAuthService().Object);

            var newClient = new ClientModel
            {
                name = "Client Three",
                address = "789 Client Rd",
                city = "ClientTown"
            };

            var result = await controller.CreateClient(newClient) as CreatedAtActionResult;
            var clients = await dbContext.ClientModels.ToListAsync();

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(3, clients.Count);
        }

        [Fact]
        public async Task UpdateClient_UpdatesExistingClient()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, GetMockAuthService().Object);

            var updatedClient = new ClientModel
            {
                name = "Updated Client One",
                address = "Updated Address"
            };

            var result = await controller.UpdateClient(1, updatedClient) as NoContentResult;
            var client = await dbContext.ClientModels.FindAsync(1);

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
            Assert.Equal("Updated Client One", client.name);
        }

        [Fact]
        public async Task DeleteClient_RemovesClient()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new ClientsService(dbContext);
            var controller = new ClientsController(service, GetMockAuthService().Object);

            var result = await controller.DeleteClient(1) as NoContentResult;
            var clients = await dbContext.ClientModels.ToListAsync();

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
            Assert.Single(clients);
        }
    }
}