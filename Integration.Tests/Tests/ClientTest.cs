using CSharpAPI.Controllers;
using CSharpAPI.Models;
using CSharpAPI.Services;
using CSharpAPI.Services.Auth;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Integration.Tests
{
    public class ClientsControllerTest : IntegrationTestBase
    {
        private readonly ClientsController _controller;
        private readonly IClientsService _service;
        private readonly IAuthService _authService;

        public ClientsControllerTest()
        {
            _service = new ClientsService(DbContext);
            _authService = new AuthService(DbContext);
            _controller = new ClientsController(_service, _authService);

            // Set up controller context with admin user
            var httpContext = new DefaultHttpContext();
            httpContext.Items["User"] = DbContext.ApiUsers.First(u => u.role == "Admin");
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            SeedTestData();
        }

        private async void SeedTestData()
        {
            // Seed test client data
            var clients = new List<ClientModel>
            {
                new ClientModel
                {
                    id = 1,
                    name = "Test Client 1",
                    address = "123 Test St",
                    city = "Test City",
                    zip_code = "12345",
                    province = "Test Province",
                    country = "Test Country",
                    contact_name = "John Doe",
                    contact_phone = "123-456-7890",
                    contact_email = "john@test.com",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ClientModel
                {
                    id = 2,
                    name = "Test Client 2",
                    address = "456 Test Ave",
                    city = "Test City 2",
                    zip_code = "67890",
                    province = "Test Province 2",
                    country = "Test Country",
                    contact_name = "Jane Smith",
                    contact_phone = "098-765-4321",
                    contact_email = "jane@test.com",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.ClientModels.AddRangeAsync(clients);

            // Seed test order data
            var orders = new List<OrderModel>
            {
                new OrderModel
                {
                    id = 1,
                    source_id = 1,
                    bill_to = 1,
                    ship_to = 1,
                    order_status = "Pending",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new OrderModel
                {
                    id = 2,
                    source_id = 2,
                    bill_to = 2,
                    ship_to = 2,
                    order_status = "Complete",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.Order.AddRangeAsync(orders);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllClients_ReturnsAllClients()
        {
            // Act
            var result = await _controller.GetAllClients();
            var resultObject = result.Result as OkObjectResult;
            var clients = resultObject?.Value as IEnumerable<ClientModel>;

            // Assert
            resultObject.Should().NotBeNull();
            resultObject.StatusCode.Should().Be(200);
            clients.Should().HaveCount(2);
            clients.Should().Contain(c => c.name == "Test Client 1");
            clients.Should().Contain(c => c.name == "Test Client 2");
        }

        [Fact]
        public async Task GetAllClients_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Items["User"] = DbContext.ApiUsers.First(u => u.role == "Viewer");
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act & Assert
            var result = await _controller.GetAllClients();
            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetClientById_ReturnsClient()
        {
            // Act
            var result = await _controller.GetClientById(1);
            var okResult = result.Result as OkObjectResult;
            var client = okResult?.Value as ClientModel;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            client.Should().NotBeNull();
            client.name.Should().Be("Test Client 1");
        }

        [Fact]
        public async Task GetClientById_ReturnsNotFound_ForInvalidId()
        {
            try
            {
                // Act
                await _controller.GetClientById(999);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Message.Should().Be("Client not found!");
            }
        }

        [Fact]
        public async Task ClientOrders_ReturnsOrders()
        {
            // Act
            var result = await _controller.ClientOrders(1);
            var okResult = result as OkObjectResult;
            var orders = okResult?.Value as List<OrderModel>;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            orders.Should().NotBeNull();
            orders.Should().Contain(o => o.bill_to == 1);
        }

        [Fact]
        public async Task CreateClient_AddsNewClient()
        {
            // Arrange
            var newClient = new ClientModel
            {
                name = "New Client",
                address = "789 New St",
                city = "New City",
                contact_email = "new@test.com"
            };

            // Act
            var result = await _controller.CreateClient(newClient);
            var createdResult = result.Result as CreatedAtActionResult;

            // Assert
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(201);

            var createdClient = createdResult.Value as ClientModel;
            createdClient.Should().NotBeNull();
            createdClient.name.Should().Be("New Client");
        }

        [Fact]
        public async Task CreateClient_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.CreateClient(null);
            var badRequestResult = result.Result as BadRequestObjectResult;

            // Assert
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Client data is null.");
        }

        [Fact]
        public async Task UpdateClient_UpdatesExistingClient()
        {
            // Arrange
            var updateClient = new ClientModel
            {
                name = "Updated Name",
                address = "Updated Address",
                contact_email = "updated@test.com"
            };

            // Act
            var result = await _controller.UpdateClient(1, updateClient);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            // Verify the update
            var updatedClient = await _service.GetClientById(1);
            updatedClient.name.Should().Be("Updated Name");
            updatedClient.contact_email.Should().Be("updated@test.com");
        }

        [Fact]
        public async Task UpdateClient_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.UpdateClient(1, null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult).Value.Should().Be("Invalid client data.");
        }

        [Fact]
        public async Task UpdateClient_ThrowsException_ForInvalidId()
        {
            // Arrange
            var updateClient = new ClientModel
            {
                name = "Updated Name"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _controller.UpdateClient(999, updateClient)
            );
            exception.Message.Should().Be("Client not found!");
        }

        [Fact]
        public async Task DeleteClient_RemovesClient()
        {
            // Act
            var result = await _controller.DeleteClient(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            // Verify deletion
            var allClients = await _service.GetAllClients();
            allClients.Should().NotContain(c => c.id == 1);
        }

        [Fact]
        public async Task DeleteClient_ThrowsException_ForInvalidId()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _controller.DeleteClient(999)
            );
            exception.Message.Should().Be("Client not found!");
        }
    }
}