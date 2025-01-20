using CSharpAPI.Controller;
using CSharpAPI.Models;
using CSharpAPI.Service;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Integration.Tests.Tests
{
    public class TransferTest : IntegrationTestBase
    {
        private readonly TransferController _controller;
        private readonly ITransfersService _service;

        public TransferTest()
        {
            _service = new TransferSerivce(DbContext);
            _controller = new TransferController(_service);

            // Seed test transfer data
            DbContext.Transfer.AddRange(
                new TransferModel
                {
                    id = 1,
                    reference = "TR001",
                    transfer_from = 1,
                    transfer_to = 2,
                    transfer_status = "Pending",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new TransferModel
                {
                    id = 2,
                    reference = "TR002",
                    transfer_from = 2,
                    transfer_to = 3,
                    transfer_status = "Completed",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            );

            DbContext.SaveChanges();
        }

        [Fact]
        public async Task GetAllTransfers_ReturnsAllTransfers()
        {
            // Act
            var result = await _controller.GetAllTransfers() as OkObjectResult;
            var transfers = result?.Value as List<TransferModel>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            transfers.Should().HaveCount(2);
            transfers.Should().Contain(t => t.reference == "TR001");
            transfers.Should().Contain(t => t.reference == "TR002");
        }

        [Fact]
        public async Task GetTransferById_ReturnsTransfer()
        {
            // Act
            var result = await _controller.GetTransferById(1) as OkObjectResult;
            var transfer = result?.Value as TransferModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            transfer.Should().NotBeNull();
            transfer.reference.Should().Be("TR001");
        }

        [Fact]
        public async Task GetTransferById_ReturnsNotFound_ForInvalidId()
        {
            // Act & Assert
            try
            {
                await _controller.GetTransferById(99);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                ex.Message.Should().Be("Transfer not found!");
            }
        }

        [Fact]
        public async Task CreateTransfer_AddsNewTransfer()
        {
            // Arrange
            var newTransfer = new TransferModel
            {
                reference = "TR003",
                transfer_from = 3,
                transfer_to = 4,
                transfer_status = "Pending",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act
            var result = await _controller.CreateTransfer(newTransfer) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            result.Value.Should().Be(newTransfer);
            result.ActionName.Should().Be(nameof(TransferController.GetTransferById));
        }

        [Fact]
        public async Task CreateTransfer_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.CreateTransfer(null) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
            result.Value.Should().Be("Request is empty!");
        }

        [Fact]
        public async Task UpdateTransfer_UpdatesExistingTransfer()
        {
            // Arrange
            var updateTransfer = new TransferModel
            {
                reference = "TR001-UPD",
                transfer_from = 1,
                transfer_to = 2,
                transfer_status = "Completed",
                updated_at = DateTime.UtcNow
            };

            // Act
            var result = await _controller.UpdateTransfer(1, updateTransfer) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Transfer 1 has been updated!");
        }

        [Fact]
        public async Task UpdateTransfer_ThrowsException_ForInvalidId()
        {
            // Arrange
            var updateTransfer = new TransferModel
            {
                reference = "TR099",
                transfer_status = "Pending"
            };

            // Act & Assert
            try
            {
                await _controller.UpdateTransfer(99, updateTransfer);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                ex.Message.Should().Be("Transfer not found!");
            }
        }

        [Fact]
        public async Task DeleteTransfer_RemovesTransfer()
        {
            // Act
            var result = await _controller.DeteteTransfer(1) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Transfer has been deleted.");

            var transfers = await _service.GetAllTransfers();
            transfers.Should().NotContain(t => t.id == 1);
        }

        [Fact]
        public async Task DeleteTransfer_ThrowsException_ForInvalidId()
        {
            // Act & Assert
            try
            {
                await _controller.DeteteTransfer(99);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                ex.Message.Should().Be("Transfer not found!");
            }
        }
    }
}