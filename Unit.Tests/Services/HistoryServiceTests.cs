using System;
using System.Threading.Tasks;
using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Unit.Tests.Services
{
    public class HistoryServiceTests : IDisposable
    {
        private readonly SQLiteDatabase _db;
        private readonly HistoryService _service;

        public HistoryServiceTests()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;
                
            _db = new SQLiteDatabase(options);
            _service = new HistoryService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task LogAsync_CreatesHistoryEntry()
        {
            // Arrange
            var entityType = EntityType.Item;
            var entityId = "123";
            var action = "Created";
            var changes = "New item created";

            // Act
            await _service.LogAsync(entityType, entityId, action, changes);

            // Assert
            var historyEntry = await _db.History.FirstOrDefaultAsync();
            Assert.NotNull(historyEntry);
            Assert.Equal(entityType, historyEntry.EntityType);
            Assert.Equal(entityId, historyEntry.EntityId);
            Assert.Equal(action, historyEntry.Action);
            Assert.Equal(changes, historyEntry.Changes);
            Assert.True(historyEntry.Timestamp <= DateTime.UtcNow);
            Assert.True(historyEntry.Timestamp >= DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task LogAsync_HandlesMultipleEntries()
        {
            // Arrange
            var entry1 = (EntityType.Item, "123", "Created", "New item");
            var entry2 = (EntityType.Order, "456", "Updated", "Status changed");

            // Act
            await _service.LogAsync(entry1.Item1, entry1.Item2, entry1.Item3, entry1.Item4);
            await _service.LogAsync(entry2.Item1, entry2.Item2, entry2.Item3, entry2.Item4);

            // Assert
            var entries = await _db.History.ToListAsync();
            Assert.Equal(2, entries.Count);
            
            Assert.Contains(entries, h => 
                h.EntityType == entry1.Item1 && 
                h.EntityId == entry1.Item2 && 
                h.Action == entry1.Item3 && 
                h.Changes == entry1.Item4);

            Assert.Contains(entries, h => 
                h.EntityType == entry2.Item1 && 
                h.EntityId == entry2.Item2 && 
                h.Action == entry2.Item3 && 
                h.Changes == entry2.Item4);
        }

        [Fact]
        public async Task LogAsync_HandlesEmptyChanges()
        {
            // Arrange
            var entityType = EntityType.Item;
            var entityId = "123";
            var action = "Viewed";
            var changes = ""; // Empty string instead of null

            // Act
            await _service.LogAsync(entityType, entityId, action, changes);

            // Assert
            var historyEntry = await _db.History.FirstOrDefaultAsync();
            Assert.NotNull(historyEntry);
            Assert.Equal(entityType, historyEntry.EntityType);
            Assert.Equal(entityId, historyEntry.EntityId);
            Assert.Equal(action, historyEntry.Action);
            Assert.Equal("", historyEntry.Changes);
        }

        [Theory]
        [InlineData(EntityType.Item, "123", "Created", "New item")]
        [InlineData(EntityType.Order, "456", "Updated", "Status changed")]
        [InlineData(EntityType.Shipment, "789", "Deleted", "Removed")]
        public async Task LogAsync_HandlesVariousEntityTypes(EntityType entityType, string entityId, string action, string changes)
        {
            // Act
            await _service.LogAsync(entityType, entityId, action, changes);

            // Assert
            var historyEntry = await _db.History.FirstOrDefaultAsync();
            Assert.NotNull(historyEntry);
            Assert.Equal(entityType, historyEntry.EntityType);
            Assert.Equal(entityId, historyEntry.EntityId);
            Assert.Equal(action, historyEntry.Action);
            Assert.Equal(changes, historyEntry.Changes);
        }

        [Fact]
        public async Task LogAsync_SetsCorrectTimestamp()
        {
            // Arrange
            var beforeTest = DateTime.UtcNow;

            // Act
            await _service.LogAsync(EntityType.Item, "123", "Created", "Test");

            // Assert
            var historyEntry = await _db.History.FirstOrDefaultAsync();
            Assert.NotNull(historyEntry);
            Assert.True(historyEntry.Timestamp >= beforeTest);
            Assert.True(historyEntry.Timestamp <= DateTime.UtcNow);
        }
    }
}
