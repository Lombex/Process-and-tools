using Moq;
using Xunit;
using CSharpAPI.Service;
using CSharpAPI.Data;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class TransferServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public TransferServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "TransfersTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task GetAllTransfers_ReturnsListOfTransfers()
        {
            // Arrange
            var transferList = new List<TransferModel>
            {
                new TransferModel { id = 1, reference = "Ref001", transfer_from = 1, transfer_to = 2, transfer_status = "Completed", created_at = DateTime.Now },
                new TransferModel { id = 2, reference = "Ref002", transfer_from = 3, transfer_to = 4, transfer_status = "Pending", created_at = DateTime.Now }
            };

            // Use a fresh context for this test
            using (var context = CreateDbContext())
            {
                context.Transfer.AddRange(transferList);
                await context.SaveChangesAsync();
            }

            // Act
            List<TransferModel> result;
            using (var context = CreateDbContext())
            {
                var service = new TransferSerivce(context);
                result = await service.GetAllTransfers();
            }

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTransferById_TransferExists_ReturnsTransfer()
        {
            // Arrange
            var transfer = new TransferModel { id = 1, reference = "Ref001", transfer_from = 1, transfer_to = 2, transfer_status = "Completed", created_at = DateTime.Now };

            using (var context = CreateDbContext())
            {
                context.Transfer.Add(transfer);
                await context.SaveChangesAsync();
            }

            // Act
            TransferModel result;
            using (var context = CreateDbContext())
            {
                var service = new TransferSerivce(context);
                result = await service.GetTransferById(1);
            }

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Ref001", result.reference);
        }

        [Fact]
        public async Task CreateTransfer_ValidTransfer_CreatesTransfer()
        {
            // Arrange
            var newTransfer = new TransferModel { reference = "Ref003", transfer_from = 1, transfer_to = 2, transfer_status = "Completed", created_at = DateTime.Now };

            // Act
            using (var context = CreateDbContext())
            {
                var service = new TransferSerivce(context);
                await service.CreateTransfer(newTransfer);
            }

            // Assert
            using (var context = CreateDbContext())
            {
                var result = await context.Transfer.FirstOrDefaultAsync(t => t.reference == "Ref003");
                Assert.NotNull(result);
                Assert.Equal("Ref003", result.reference);
            }
        }
    }
}
