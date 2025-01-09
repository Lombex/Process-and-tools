using Xunit;
using Moq;
using CSharpAPI.Models;
using CSharpAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class ClientsServiceUnitTests
    {
        [Fact]
        public async Task GetAllClients_ReturnsListOfClients()
        {
            // Arrange
            var mockClients = new List<ClientModel>
            {
                new ClientModel { id = 1, name = "Client A" },
                new ClientModel { id = 2, name = "Client B" }
            };

            var mockService = new Mock<IClientsService>();
            mockService
                .Setup(service => service.GetAllClients())
                .ReturnsAsync(mockClients);

            // Act
            var result = await mockService.Object.GetAllClients();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetClientById_ValidId_ReturnsClient()
        {
            // Arrange
            var mockClient = new ClientModel { id = 1, name = "Client A" };

            var mockService = new Mock<IClientsService>();
            mockService
                .Setup(service => service.GetClientById(1))
                .ReturnsAsync(mockClient);

            // Act
            var result = await mockService.Object.GetClientById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
        }

        [Fact]
        public async Task AddClient_ValidClient_CallsAddMethod()
        {
            // Arrange
            var newClient = new ClientModel { name = "Client C" };

            var mockService = new Mock<IClientsService>();
            mockService
                .Setup(service => service.AddClient(It.IsAny<ClientModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockService.Object.AddClient(newClient);

            // Assert
            mockService.Verify(service => service.AddClient(It.Is<ClientModel>(c => c.name == "Client C")), Times.Once);
        }

        [Fact]
        public async Task UpdateClient_ValidClient_CallsUpdateMethod()
        {
            // Arrange
            var updatedClient = new ClientModel { name = "Updated Client A" };

            var mockService = new Mock<IClientsService>();
            mockService
                .Setup(service => service.UpdateClient(1, It.IsAny<ClientModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockService.Object.UpdateClient(1, updatedClient);

            // Assert
            mockService.Verify(service => service.UpdateClient(1, It.Is<ClientModel>(c => c.name == "Updated Client A")), Times.Once);
        }

        [Fact]
        public async Task DeleteClient_ValidId_CallsDeleteMethod()
        {
            // Arrange
            var mockService = new Mock<IClientsService>();
            mockService
                .Setup(service => service.DeleteClient(1))
                .Returns(Task.CompletedTask);

            // Act
            await mockService.Object.DeleteClient(1);

            // Assert
            mockService.Verify(service => service.DeleteClient(1), Times.Once);
        }
    }
}
