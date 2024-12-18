using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Controller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class Transfers_Testing
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed initial data
            context.Transfer.AddRange(
                new TransferModel
                {
                    id = 1,
                    reference = "TR001",
                    transfer_from = 2,
                    transfer_to = 1,
                    transfer_status = "Pending",
                    updated_at = DateTime.Now,
                    items = new List<Items>
                    {
                        new Items { amount =8},
                        new Items { amount = 6}
                    }
                },
                new TransferModel
                {
                    id = 2,
                    reference = "TR002",
                    transfer_from = 3,
                    transfer_to = 4,
                    transfer_status = "Completed",
                    updated_at = DateTime.Now,
                    items = new List<Items>
                    {
                        new Items { amount = 9}
                    }
                }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAllTransfers_ReturnsAllTransfers()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new TransferSerivce(dbContext);
            var controller = new TransferController(service);

            var result = await controller.GetAllTransfers() as OkObjectResult;
            var transfers = result?.Value as List<TransferModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, transfers.Count);
        }

        [Fact]
        public async Task GetTransferById_ReturnsTransfer()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new TransferSerivce(dbContext);
            var controller = new TransferController(service);

            var result = await controller.GetTransferById(1) as OkObjectResult;
            var transfer = result?.Value as TransferModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(transfer);
            Xunit.Assert.Equal("TR001", transfer.reference);
        }

        [Fact]
        public async Task GetTransferById_ReturnsNotFound()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new TransferSerivce(dbContext);
            var controller = new TransferController(service);

            var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => controller.GetTransferById(99));
            Xunit.Assert.Equal("Transfer not found!", exception.Message);
        }

        [Fact]
        public async Task GetItemFromTransferId_ReturnsItems()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new TransferSerivce(dbContext);
            var controller = new TransferController(service);

            var result = await controller.GetItemFromTransferId(1) as OkObjectResult;
            var items = result?.Value as List<Items>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task CreateTransfer_AddsNewTransfer()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new TransferSerivce(dbContext);
            var controller = new TransferController(service);

            var newTransfer = new TransferModel
            {
                id = 3,
                reference = "TR003",
                transfer_from = 1,
                transfer_to = 5,
                transfer_status = "Pending",
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { amount = 7 }
                }
            };

            var result = await controller.CreateTransfer(newTransfer) as CreatedAtActionResult;
            var transfers = await dbContext.Transfer.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(3, transfers.Count);
            Xunit.Assert.Contains(transfers, t => t.reference == "TR003");
        }

        [Fact]
        public async Task UpdateTransfer_UpdatesExistingTransfer()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new TransferSerivce(dbContext);
            var controller = new TransferController(service);

            var updatedTransfer = new TransferModel
            {
                reference = "TR001-Updated",
                transfer_from = 1,
                transfer_to = 7,
                transfer_status = "In Progress",
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { amount = 4 }
                }
            };

            var result = await controller.UpdateTransfer(1, updatedTransfer) as OkObjectResult;
            var transfer = await dbContext.Transfer.FirstOrDefaultAsync(t => t.id == 1);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(transfer);
            Xunit.Assert.Equal("TR001-Updated", transfer.reference);
        }

        [Fact]
        public async Task DeleteTransfer_RemovesTransfer()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new TransferSerivce(dbContext);
            var controller = new TransferController(service);

            var result = await controller.DeteteTransfer(1) as OkObjectResult;
            var transfers = await dbContext.Transfer.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(transfers);
            Xunit.Assert.DoesNotContain(transfers, t => t.id == 1);
        }
    }
}
