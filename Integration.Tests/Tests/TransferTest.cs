using CSharpAPI.Controller;
using CSharpAPI.Models;
using CSharpAPI.Service;
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
    public class TransferTest : IntegrationTestBase
    {
        private readonly TransferController _controller;
        private readonly ITransfersService _service;
        private readonly IAuthService _authService;

        public TransferTest()
        {
            _service = new TransferSerivce(DbContext);
            _authService = new AuthService(DbContext, Configuration);
            _controller = new TransferController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.Transfer.RemoveRange(DbContext.Transfer);
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var transfers = new List<TransferModel>
            {
                new TransferModel
                {
                    reference = "TR001",
                    transfer_from = 1,
                    transfer_to = 2,
                    transfer_status = "Pending",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow,
                    items = new List<Items>()
                },
                new TransferModel
                {
                    reference = "TR002",
                    transfer_from = 2,
                    transfer_to = 3,
                    transfer_status = "Completed",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow,
                    items = new List<Items>()
                }
            };

            await DbContext.Transfer.AddRangeAsync(transfers);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllTransfers_ReturnsAllTransfers_WhenAuthorized()
        {
            // Act
            var actionResult = await _controller.GetAllTransfers(0);
            
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
            var transfersProperty = responseType.GetProperty("Transfers").GetValue(response) as IEnumerable<object>;

            pageProperty.Should().Be(0);
            pageSizeProperty.Should().Be(10);
            totalItemsProperty.Should().Be(2);
            totalPagesProperty.Should().Be(1);

            transfersProperty.Should().NotBeNull();
            var transfers = transfersProperty.ToList();
            transfers.Should().HaveCount(2);

            // Check first transfer
            var firstTransfer = transfers.First();
            var firstTransferType = firstTransfer.GetType();
            (firstTransferType.GetProperty("Reference").GetValue(firstTransfer) as string).Should().Be("TR001");
            
            // Check second transfer
            var secondTransfer = transfers.Last();
            var secondTransferType = secondTransfer.GetType();
            (secondTransferType.GetProperty("Reference").GetValue(secondTransfer) as string).Should().Be("TR002");
        }

        [Fact]
        public async Task GetAllTransfers_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            // Act
            var result = await _controller.GetAllTransfers(0);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetTransferById_ReturnsTransfer_WhenAuthorized()
        {
            // Arrange
            var transfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
            transfer.Should().NotBeNull();

            // Act
            var result = await _controller.GetTransferById(transfer.id) as OkObjectResult;
            var returnedTransfer = result?.Value as TransferModel;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedTransfer.Should().NotBeNull();
            returnedTransfer.reference.Should().Be("TR001");
            returnedTransfer.transfer_status.Should().Be("Pending");
        }

        [Fact]
        public async Task GetTransferById_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var transfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
            transfer.Should().NotBeNull();

            // Act
            var result = await _controller.GetTransferById(transfer.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task CreateTransfer_AddsNewTransfer_WhenAuthorized()
        {
            // Arrange
            var newTransfer = new TransferModel
            {
                reference = "TR003",
                transfer_from = 3,
                transfer_to = 4,
                transfer_status = "Pending",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                items = new List<Items>()
            };

            // Act
            var result = await _controller.CreateTransfer(newTransfer) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdTransfer = result.Value as TransferModel;
            createdTransfer.Should().NotBeNull();
            createdTransfer.reference.Should().Be("TR003");
            createdTransfer.transfer_status.Should().Be("Pending");
        }

        [Fact]
        public async Task CreateTransfer_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var newTransfer = new TransferModel
            {
                reference = "TR003",
                transfer_status = "Pending",
                items = new List<Items>()
            };

            // Act
            var result = await _controller.CreateTransfer(newTransfer);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task UpdateTransfer_UpdatesExistingTransfer_WhenAuthorized()
        {
            // Arrange
            var transfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
            transfer.Should().NotBeNull();

            var updateTransfer = new TransferModel
            {
                reference = "TR001-UPD",
                transfer_from = 1,
                transfer_to = 2,
                transfer_status = "Completed",
                items = new List<Items>()
            };

            // Act
            var result = await _controller.UpdateTransfer(transfer.id, updateTransfer) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be($"Transfer {transfer.id} has been updated!");

            var updatedTransfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.id == transfer.id);
            updatedTransfer.Should().NotBeNull();
            updatedTransfer.reference.Should().Be("TR001-UPD");
            updatedTransfer.transfer_status.Should().Be("Completed");
        }

        [Fact]
        public async Task UpdateTransfer_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var transfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
            transfer.Should().NotBeNull();

            var updateTransfer = new TransferModel
            {
                reference = "TR001-UPD",
                transfer_status = "Completed"
            };

            // Act
            var result = await _controller.UpdateTransfer(transfer.id, updateTransfer);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task DeleteTransfer_RemovesTransfer_WhenAuthorized()
        {
            // Arrange
            var transfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
            transfer.Should().NotBeNull();

            // Act
            var result = await _controller.DeteteTransfer(transfer.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be("Transfer has been deleted.");

            var deletedTransfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.id == transfer.id);
            deletedTransfer.Should().BeNull();
        }

        [Fact]
        public async Task DeleteTransfer_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var transfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
            transfer.Should().NotBeNull();

            // Act
            var result = await _controller.DeteteTransfer(transfer.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task CommitTransfer_CompletesTransfer_WhenAuthorized()
        {
            // Arrange
            var transfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
            transfer.Should().NotBeNull();

            // Act
            var result = await _controller.CommitTransfer(transfer.id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be($"Transfer {transfer.id} has been Completed and processed.");
        }

//        [Fact]
//        public async Task TransferToLocation_ProcessesTransfer_WhenAuthorized()
//        {
//            try
//            {
//                // Arrange
//                var transfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
//                transfer.Should().NotBeNull();
//                var locationId = 1; // Use a valid location ID that exists in the database
//
//                // Act
//                var result = await _controller.TransferToLocation(transfer.id, locationId);
//
//                // Assert
//                result.Should().BeOfType<OkObjectResult>();
//                var okResult = result as OkObjectResult;
//                okResult.Should().NotBeNull();
//                okResult.StatusCode.Should().Be(200);
//                okResult.Value.Should().Be($"Transfer {transfer.id} has been processed to location {locationId}");
//            }
//            catch (Exception ex)
//            {
//                // If there's an error, it should be returned as BadRequest
//                ex.Should().BeOfType<BadRequestObjectResult>();
//            }
//        }

        [Fact]
        public async Task TransferToLocation_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var transfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
            transfer.Should().NotBeNull();
            var locationId = 1;

            // Act
            var result = await _controller.TransferToLocation(transfer.id, locationId);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task TransferFromLocation_ProcessesTransfer_WhenAuthorized()
        {
            try
            {
                // Arrange
                var transfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
                transfer.Should().NotBeNull();
                var locationId = 1; // Use a valid location ID that exists in the database

                // Act
                var result = await _controller.TransferFromLocation(transfer.id, locationId);

                // Assert
                result.Should().BeOfType<OkObjectResult>();
                var okResult = result as OkObjectResult;
                okResult.Should().NotBeNull();
                okResult.StatusCode.Should().Be(200);
                okResult.Value.Should().Be($"Transfer {transfer.id} has been processed from location {locationId}");
            }
            catch (Exception ex)
            {
                // If there's an error, it should be returned as BadRequest
                ex.Should().BeOfType<BadRequestObjectResult>();
            }
        }

        [Fact]
        public async Task TransferFromLocation_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var transfer = await DbContext.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
            transfer.Should().NotBeNull();
            var locationId = 1;

            // Act
            var result = await _controller.TransferFromLocation(transfer.id, locationId);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }
    }
}
