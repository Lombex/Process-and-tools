using CSharpAPI.Controllers;
using CSharpAPI.Models;
using CSharpAPI.Services;
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
    public class ClientsControllerTest : IntegrationTestBase
    {
        private readonly ClientsController _controller;
        private readonly IClientsService _service;
        private readonly IAuthService _authService;

        public ClientsControllerTest()
        {
            _service = new ClientsService(DbContext);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new ClientsController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.ClientModels.RemoveRange(DbContext.ClientModels);
            DbContext.Order.RemoveRange(DbContext.Order);
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            // Seed test client data
            var clients = new List<ClientModel>
            {
                new ClientModel
                {
                    name = "Test Client 1",
                    address = "123 Test St",
                    city = "Test City",
                    zip_code = "12345",
                    province = "Test Province",
                    country = "Test Country",
                    contact = new Contact
                    {
                        name = "John Doe",
                        phone = "123-456-7890",
                        email = "john@test.com"
                    },
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ClientModel
                {
                    name = "Test Client 2",
                    address = "456 Test Ave",
                    city = "Test City 2",
                    zip_code = "67890",
                    province = "Test Province 2",
                    country = "Test Country",
                    contact = new Contact
                    {
                        name = "Jane Smith",
                        phone = "098-765-4321",
                        email = "jane@test.com"
                    },
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.ClientModels.AddRangeAsync(clients);
            await DbContext.SaveChangesAsync();

            var client1 = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.name == "Test Client 1");
            var client2 = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.name == "Test Client 2");

            // Seed test order data
            var orders = new List<OrderModel>
            {
                new OrderModel
                {
                    source_id = 1,
                    bill_to = client1.id,
                    ship_to = client1.id,
                    order_status = "Pending",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new OrderModel
                {
                    source_id = 2,
                    bill_to = client2.id,
                    ship_to = client2.id,
                    order_status = "Complete",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.Order.AddRangeAsync(orders);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllClients_ReturnsAllClients_WhenAuthorized()
        {
            // Act
            var actionResult = await _controller.GetAllClients(0);
            
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
            var clientsProperty = responseType.GetProperty("Client").GetValue(response) as IEnumerable<object>;

            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(2);
            totalPagesProperty.Should().Be(1);

            clientsProperty.Should().NotBeNull();
            var clients = clientsProperty.ToList();
            clients.Should().HaveCount(2);

            // Check first client
            var firstClient = clients.First();
            var firstClientType = firstClient.GetType();
            (firstClientType.GetProperty("Name").GetValue(firstClient) as string).Should().Be("Test Client 1");
            
            // Check second client
            var secondClient = clients.Last();
            var secondClientType = secondClient.GetType();
            (secondClientType.GetProperty("Name").GetValue(secondClient) as string).Should().Be("Test Client 2");
        }

        [Fact]
        public async Task GetAllClients_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            // Act
            var result = await _controller.GetAllClients(0);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetClientById_ReturnsClient_WhenAuthorized()
        {
            // Arrange
            var client = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.name == "Test Client 1");
            client.Should().NotBeNull();

            // Act
            var actionResult = await _controller.GetClientById(client.id);
            var result = actionResult.Result as OkObjectResult;
            var returnedClient = result?.Value as ClientModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedClient.Should().NotBeNull();
            returnedClient.name.Should().Be("Test Client 1");
            returnedClient.contact.name.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetClientById_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var client = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.name == "Test Client 1");
            client.Should().NotBeNull();

            // Act
            var result = await _controller.GetClientById(client.id);

            // Assert
            result.Result.Should().BeOfType<ForbidResult>();
        }

//        [Fact]
//        public async Task GetClientById_ReturnsNotFound_ForInvalidId()
//        {
//            // Arrange
//            var invalidId = 999;
//        
//            // Act
//            var actionResult = await _controller.GetClientById(invalidId);
//            
//            // Assert
//            actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
//            var notFoundResult = actionResult.Result as NotFoundObjectResult;
//            notFoundResult.Should().NotBeNull();
//            notFoundResult.StatusCode.Should().Be(404);
//            notFoundResult.Value.Should().Be($"Client with id {invalidId} not found.");
//        }
        

        [Fact]
        public async Task ClientOrders_ReturnsOrders_WhenAuthorized()
        {
            // Arrange
            var client = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.name == "Test Client 1");
            client.Should().NotBeNull();

            // Act
            var result = await _controller.ClientOrders(client.id) as OkObjectResult;
            var orders = result?.Value as List<OrderModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            orders.Should().NotBeNull();
            orders.Should().OnlyContain(o => o.bill_to == client.id);
        }

        [Fact]
        public async Task ClientOrders_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var client = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.name == "Test Client 1");
            client.Should().NotBeNull();

            // Act
            var result = await _controller.ClientOrders(client.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task CreateClient_AddsNewClient_WhenAuthorized()
        {
            // Arrange
            var newClient = new ClientModel
            {
                name = "New Client",
                address = "789 New St",
                city = "New City",
                contact = new Contact
                {
                    name = "New Contact",
                    email = "new@test.com",
                    phone = "555-555-5555"
                },
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act
            var actionResult = await _controller.CreateClient(newClient);
            var result = actionResult.Result as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdClient = result.Value as ClientModel;
            createdClient.Should().NotBeNull();
            createdClient.name.Should().Be("New Client");
            createdClient.contact.email.Should().Be("new@test.com");
        }

        [Fact]
        public async Task CreateClient_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var newClient = new ClientModel
            {
                name = "New Client",
                contact = new Contact { email = "new@test.com" }
            };

            // Act
            var result = await _controller.CreateClient(newClient);

            // Assert
            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task UpdateClient_UpdatesExistingClient_WhenAuthorized()
        {
            // Arrange
            var client = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.name == "Test Client 1");
            client.Should().NotBeNull();

            var updateClient = new ClientModel
            {
                name = "Updated Name",
                address = "Updated Address",
                contact = new Contact
                {
                    name = "Updated Contact",
                    email = "updated@test.com"
                }
            };

            // Act
            var result = await _controller.UpdateClient(client.id, updateClient);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            var updatedClient = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.id == client.id);
            updatedClient.Should().NotBeNull();
            updatedClient.name.Should().Be("Updated Name");
            updatedClient.contact.email.Should().Be("updated@test.com");
        }

        [Fact]
        public async Task UpdateClient_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var client = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.name == "Test Client 1");
            client.Should().NotBeNull();

            var updateClient = new ClientModel
            {
                name = "Updated Name"
            };

            // Act
            var result = await _controller.UpdateClient(client.id, updateClient);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task DeleteClient_RemovesClient_WhenAuthorized()
        {
            // Arrange
            var client = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.name == "Test Client 1");
            client.Should().NotBeNull();

            // Act
            var result = await _controller.DeleteClient(client.id);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            var deletedClient = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.id == client.id);
            deletedClient.Should().BeNull();
        }

        [Fact]
        public async Task DeleteClient_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var client = await DbContext.ClientModels.FirstOrDefaultAsync(c => c.name == "Test Client 1");
            client.Should().NotBeNull();

            // Act
            var result = await _controller.DeleteClient(client.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }
    }
}
