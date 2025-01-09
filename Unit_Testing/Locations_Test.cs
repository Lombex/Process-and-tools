using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class LocationServiceUnitTests
    {
        [Fact]
        public async Task GetAll_ReturnsListOfLocations()
        {
            // Arrange
            var mockLocations = new List<LocationModel>
            {
                new LocationModel { id = 1, warehouse_id = 1, code = "A.1.0", name = "Row: A, Rack: 1, Shelf: 0" },
                new LocationModel { id = 2, warehouse_id = 1, code = "A.2.0", name = "Row: A, Rack: 2, Shelf: 0" }
            };

            var mockLocationService = new Mock<ILocationService>();
            mockLocationService
                .Setup(service => service.GetAll())
                .ReturnsAsync(mockLocations);

            // Act
            var result = await mockLocationService.Object.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsLocation()
        {
            // Arrange
            var location = new LocationModel
            {
                id = 1,
                warehouse_id = 1,
                code = "A.1.0",
                name = "Row: A, Rack: 1, Shelf: 0"
            };

            var mockLocationService = new Mock<ILocationService>();
            mockLocationService
                .Setup(service => service.GetById(1))
                .ReturnsAsync(location);

            // Act
            var result = await mockLocationService.Object.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
        }

        [Fact]
        public async Task Add_ValidLocation_CallsAddMethod()
        {
            // Arrange
            var newLocation = new LocationModel
            {
                warehouse_id = 3,
                code = "B.1.0",
                name = "Row: B, Rack: 1, Shelf: 0"
            };

            var mockLocationService = new Mock<ILocationService>();
            mockLocationService
                .Setup(service => service.Add(It.IsAny<LocationModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockLocationService.Object.Add(newLocation);

            // Assert
            mockLocationService.Verify(service => service.Add(It.Is<LocationModel>(l => l.code == "B.1.0")), Times.Once);
        }

        [Fact]
        public async Task Update_ValidLocation_CallsUpdateMethod()
        {
            // Arrange
            var updatedLocation = new LocationModel
            {
                warehouse_id = 1,
                code = "A.1.0",
                name = "Updated Row: A, Rack: 1, Shelf: 0"
            };

            var mockLocationService = new Mock<ILocationService>();
            mockLocationService
                .Setup(service => service.Update(It.IsAny<int>(), It.IsAny<LocationModel>()))
                .ReturnsAsync(true);

            // Act
            var result = await mockLocationService.Object.Update(1, updatedLocation);

            // Assert
            Assert.True(result);
            mockLocationService.Verify(service => service.Update(1, It.Is<LocationModel>(l => l.name == "Updated Row: A, Rack: 1, Shelf: 0")), Times.Once);
        }

        [Fact]
        public async Task Delete_ValidId_CallsDeleteMethod()
        {
            // Arrange
            var mockLocationService = new Mock<ILocationService>();
            mockLocationService
                .Setup(service => service.Delete(1))
                .ReturnsAsync(true);

            // Act
            var result = await mockLocationService.Object.Delete(1);

            // Assert
            Assert.True(result);
            mockLocationService.Verify(service => service.Delete(1), Times.Once);
        }
    }
}
