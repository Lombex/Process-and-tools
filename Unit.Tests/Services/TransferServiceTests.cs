using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Unit.Tests.Services
{
    public class TransferServiceTests : IDisposable
    {
        private readonly SQLiteDatabase _db;
        private readonly ITransfersService _service;

        public TransferServiceTests()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;
                
            _db = new SQLiteDatabase(options);
            _service = new TransferSerivce(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetAllTransfers_ReturnsAllTransfers()
        {
            // Arrange
            var transfers = new List<TransferModel>
            {
                new TransferModel 
                { 
                    id = 1, 
                    reference = "TR001", 
                    transfer_status = "Pending",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new TransferModel 
                { 
                    id = 2, 
                    reference = "TR002", 
                    transfer_status = "Completed",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };
            await _db.Transfer.AddRangeAsync(transfers);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAllTransfers();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.reference == "TR001");
            Assert.Contains(result, t => t.reference == "TR002");
        }

        [Fact]
        public async Task GetTransferById_ReturnsCorrectTransfer()
        {
            // Arrange
            var transfer = new TransferModel 
            { 
                id = 1, 
                reference = "TR001",
                transfer_status = "Pending",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await _db.Transfer.AddAsync(transfer);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetTransferById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transfer.reference, result.reference);
        }

        [Fact]
        public async Task GetTransferById_ThrowsException_WhenNotFound()
        {
            await Assert.ThrowsAsync<Exception>(() => _service.GetTransferById(1));
        }

        [Fact]
        public async Task GetItemFromTransferId_ReturnsCorrectItems()
        {
            // Arrange
            var items = new List<Items>
            {
                new Items { item_id = "ITEM001", amount = 5 },
                new Items { item_id = "ITEM002", amount = 3 }
            };

            var transfer = new TransferModel
            {
                id = 1,
                reference = "TR001",
                items = items,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            await _db.Transfer.AddAsync(transfer);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetItemFromTransferId(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, i => i.item_id == "ITEM001");
            Assert.Contains(result, i => i.item_id == "ITEM002");
        }

        [Fact]
        public async Task CreateTransfer_AddsNewTransfer()
        {
            // Arrange
            var transfer = new TransferModel
            {
                reference = "TR001",
                transfer_from = 1,
                transfer_to = 2,
                transfer_status = "Pending",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                items = new List<Items>
                {
                    new Items { item_id = "ITEM001", amount = 5 }
                }
            };

            // Act
            await _service.CreateTransfer(transfer);

            // Assert
            var result = await _db.Transfer.FirstOrDefaultAsync(t => t.reference == "TR001");
            Assert.NotNull(result);
            Assert.Equal(transfer.transfer_from, result.transfer_from);
            Assert.Equal(transfer.transfer_to, result.transfer_to);
        }

        [Fact]
        public async Task CreateTransfer_ThrowsException_WhenTransferIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateTransfer(null));
        }

        [Fact]
        public async Task UpdateTransfer_UpdatesExistingTransfer()
        {
            // Arrange
            var transfer = new TransferModel
            {
                id = 1,
                reference = "TR001",
                transfer_status = "Pending",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await _db.Transfer.AddAsync(transfer);
            await _db.SaveChangesAsync();

            var updatedTransfer = new TransferModel
            {
                reference = "TR001-Updated",
                transfer_status = "Completed",
                updated_at = DateTime.UtcNow
            };

            // Act
            await _service.UpdateTransfer(1, updatedTransfer);

            // Assert
            var result = await _db.Transfer.FindAsync(1);
            Assert.Equal(updatedTransfer.reference, result.reference);
            Assert.Equal(updatedTransfer.transfer_status, result.transfer_status);
        }

        [Fact]
        public async Task UpdateTransfer_ThrowsException_WhenTransferNotFound()
        {
            var updateTransfer = new TransferModel { reference = "TR001" };
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateTransfer(999, updateTransfer));
        }

        [Fact]
        public async Task CommitTransfer_UpdatesInventoryAndStatus()
        {
            // Arrange
            var transfer = new TransferModel
            {
                id = 1,
                reference = "TR001",
                transfer_from = 1,
                transfer_to = 2,
                transfer_status = "Pending",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                items = new List<Items>
                {
                    new Items { item_id = "ITEM001", amount = 5 }
                }
            };

            var inventory1 = new InventorieModel
            {
                item_id = "ITEM001",
                locations = new List<int> { 1 },
                total_on_hand = 10,
                total_allocated = 0,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            var inventory2 = new InventorieModel
            {
                item_id = "ITEM001",
                locations = new List<int> { 2 },
                total_on_hand = 0,
                total_allocated = 0,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            await _db.Transfer.AddAsync(transfer);
            await _db.Inventors.AddRangeAsync(new[] { inventory1, inventory2 });
            await _db.SaveChangesAsync();

            // Act
            await _service.CommitTransfer(1);

            // Assert
            var updatedTransfer = await _db.Transfer.FindAsync(1);
            Assert.Equal("Completed", updatedTransfer.transfer_status);

            var sourceInventory = await _db.Inventors.FirstAsync(i => i.locations.Contains(1));
            var destInventory = await _db.Inventors.FirstAsync(i => i.locations.Contains(2));

            Assert.Equal(5, sourceInventory.total_on_hand);
            Assert.Equal(5, destInventory.total_on_hand);
        }

        [Fact]
        public async Task CommitTransfer_ThrowsException_WhenTransferNotFound()
        {
            await Assert.ThrowsAsync<Exception>(() => _service.CommitTransfer(1));
        }

        [Fact]
        public async Task CommitTransfer_ThrowsException_WhenTransferNotPending()
        {
            // Arrange
            var transfer = new TransferModel
            {
                id = 1,
                transfer_status = "Completed",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await _db.Transfer.AddAsync(transfer);
            await _db.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CommitTransfer(1));
        }

        [Fact]
        public async Task TransferToLocation_UpdatesTransferToLocation()
        {
            // Arrange
            var transfer = new TransferModel 
            { 
                id = 1,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            var location = new LocationModel 
            { 
                id = 2,
                code = "LOC01",
                name = "Test Location",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await _db.Transfer.AddAsync(transfer);
            await _db.Location.AddAsync(location);
            await _db.SaveChangesAsync();

            // Act
            await _service.TransferToLocation(1, 2);

            // Assert
            var result = await _db.Transfer.FindAsync(1);
            Assert.Equal(2, result.transfer_to);
        }

        [Fact]
        public async Task TransferFromLocation_UpdatesFromExistingLocation()
        {
            // Arrange
            var transfer = new TransferModel 
            { 
                id = 1,
                transfer_from = 1,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            var location = new LocationModel 
            { 
                id = 2,
                code = "LOC01",
                name = "Test Location",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await _db.Transfer.AddAsync(transfer);
            await _db.Location.AddAsync(location);
            await _db.SaveChangesAsync();

            // Act
            await _service.TransferFromLocation(1, 2);

            // Assert
            var result = await _db.Transfer.FindAsync(1);
            Assert.Equal(2, result.transfer_from);
        }

        [Fact]
        public async Task TransferFromLocation_UpdatesFromDock()
        {
            // Arrange
            var transfer = new TransferModel 
            { 
                id = 1,
                transfer_from = null,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            var dock = new DockModel 
            { 
                id = 2,
                warehouse_id = 1,
                code = "DOCK01",
                name = "Test Dock",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await _db.Transfer.AddAsync(transfer);
            await _db.DockModels.AddAsync(dock);
            await _db.SaveChangesAsync();

            // Act
            await _service.TransferFromLocation(1, 2);

            // Assert
            var result = await _db.Transfer.FindAsync(1);
            Assert.Equal(2, result.transfer_from);
        }

        [Fact]
        public async Task TransferFromLocation_ThrowsException_WhenDockNotFound()
        {
            // Arrange
            var transfer = new TransferModel 
            { 
                id = 1,
                transfer_from = null,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await _db.Transfer.AddAsync(transfer);
            await _db.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.TransferFromLocation(1, 999)
            );
            Assert.Equal("Dock not found!", exception.Message);
        }

        [Fact]
        public async Task Delete_DeletesAndArchivesTransfer()
        {
            // Arrange
            var transfer = new TransferModel
            {
                id = 1,
                reference = "TR001",
                transfer_status = "Pending",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await _db.Transfer.AddAsync(transfer);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteTransfer(1);

            // Assert
            var deletedTransfer = await _db.Transfer.FindAsync(1);
            Assert.Null(deletedTransfer);

            var archivedTransfer = await _db.ArchivedTransfers.FirstOrDefaultAsync();
            Assert.NotNull(archivedTransfer);
            Assert.Equal(transfer.reference, archivedTransfer.reference);
            Assert.Equal(transfer.transfer_status, archivedTransfer.transfer_status);
        }

        [Fact]
        public async Task Delete_ThrowsException_WhenTransferNotFound()
        {
            await Assert.ThrowsAsync<Exception>(() => _service.DeleteTransfer(999));
        }
    }
}
