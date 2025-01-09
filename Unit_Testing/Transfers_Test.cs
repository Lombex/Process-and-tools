using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class TransferServiceUnitTests
    {
        [Fact]
        public async Task GetAllTransfers_ReturnsListOfTransfers()
        {
            // Arrange
            var mockTransfers = new List<TransferModel>
            {
                new TransferModel { id = 1, reference = "Ref001", transfer_from = 1, transfer_to = 2, transfer_status = "Completed" },
                new TransferModel { id = 2, reference = "Ref002", transfer_from = 3, transfer_to = 4, transfer_status = "Pending" }
            };

            var mockTransferService = new Mock<ITransfersService>();
            mockTransferService
                .Setup(service => service.GetAllTransfers())
                .ReturnsAsync(mockTransfers);

            // Act
            var result = await mockTransferService.Object.GetAllTransfers();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task CreateTransfer_ValidTransfer_CallsCreateMethod()
        {
            // Arrange
            var newTransfer = new TransferModel
            {
                reference = "Ref003",
                transfer_from = 1,
                transfer_to = 2,
                transfer_status = "Completed"
            };

            var mockTransferService = new Mock<ITransfersService>();
            mockTransferService
                .Setup(service => service.CreateTransfer(It.IsAny<TransferModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockTransferService.Object.CreateTransfer(newTransfer);

            // Assert
            mockTransferService.Verify(service => service.CreateTransfer(It.Is<TransferModel>(t => t.reference == "Ref003")), Times.Once);
        }

        [Fact]
        public async Task GetTransferById_ValidId_ReturnsTransfer()
        {
            // Arrange
            var transfer = new TransferModel
            {
                id = 1,
                reference = "Ref001",
                transfer_from = 1,
                transfer_to = 2,
                transfer_status = "Completed"
            };

            var mockTransferService = new Mock<ITransfersService>();
            mockTransferService
                .Setup(service => service.GetTransferById(1))
                .ReturnsAsync(transfer);

            // Act
            var result = await mockTransferService.Object.GetTransferById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Ref001", result.reference);
        }

        [Fact]
        public async Task UpdateTransfer_ValidTransfer_CallsUpdateMethod()
        {
            // Arrange
            var updatedTransfer = new TransferModel
            {
                id = 1,
                reference = "UpdatedRef",
                transfer_from = 1,
                transfer_to = 3,
                transfer_status = "Pending"
            };

            var mockTransferService = new Mock<ITransfersService>();
            mockTransferService
                .Setup(service => service.UpdateTransfer(It.IsAny<int>(), It.IsAny<TransferModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockTransferService.Object.UpdateTransfer(1, updatedTransfer);

            // Assert
            mockTransferService.Verify(service => service.UpdateTransfer(1, It.Is<TransferModel>(t => t.reference == "UpdatedRef" && t.transfer_status == "Pending")), Times.Once);
        }

        [Fact]
        public async Task DeleteTransfer_ValidId_CallsDeleteMethod()
        {
            // Arrange
            var mockTransferService = new Mock<ITransfersService>();
            mockTransferService
                .Setup(service => service.DeleteTransfer(1))
                .Returns(Task.CompletedTask);

            // Act
            await mockTransferService.Object.DeleteTransfer(1);

            // Assert
            mockTransferService.Verify(service => service.DeleteTransfer(1), Times.Once);
        }

        [Fact]
        public async Task GetItemFromTransferId_ValidId_ReturnsItemsList()
        {
            // Arrange
            var mockItems = new List<Items>
            {
                new Items { item_id = "1", amount = 5 },
                new Items { item_id = "2", amount = 10 }
            };

            var transfer = new TransferModel
            {
                id = 1,
                reference = "Ref001",
                items = mockItems
            };

            var mockTransferService = new Mock<ITransfersService>();
            mockTransferService
                .Setup(service => service.GetTransferById(1))
                .ReturnsAsync(transfer);

            mockTransferService
                .Setup(service => service.GetItemFromTransferId(1))
                .ReturnsAsync(mockItems);

            // Act
            var result = await mockTransferService.Object.GetItemFromTransferId(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("1", result[0].item_id);
            Assert.Equal(5, result[0].amount);
        }

    }
}
