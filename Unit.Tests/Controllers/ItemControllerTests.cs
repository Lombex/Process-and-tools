using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Unit.Tests.Services
{
    public class ItemGroupServiceTests : IDisposable
    {
        private readonly SQLiteDatabase _db;
        private readonly ItemGroupService _service;

        public ItemGroupServiceTests()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;
                
            _db = new SQLiteDatabase(options);
            _service = new ItemGroupService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetAll_ReturnsAllItemGroups()
        {
            // Arrange
            var expectedItemGroups = new List<ItemGroupModel>
            {
                new ItemGroupModel { id = 1, name = "Group1" },
                new ItemGroupModel { id = 2, name = "Group2" }
            };

            await _db.ItemGroups.AddRangeAsync(expectedItemGroups);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.Equal(expectedItemGroups.Count, result.Count);
            Assert.Equal(expectedItemGroups[0].id, result[0].id);
            Assert.Equal(expectedItemGroups[1].id, result[1].id);
        }

        [Fact]
        public async Task GetById_ReturnsCorrectItemGroup()
        {
            // Arrange
            var expectedItemGroup = new ItemGroupModel { id = 1, name = "Group1" };
            await _db.ItemGroups.AddAsync(expectedItemGroup);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetById(1);

            // Assert
            Assert.Equal(expectedItemGroup.id, result.id);
            Assert.Equal(expectedItemGroup.name, result.name);
        }

        [Fact]
        public async Task GetById_ThrowsException_WhenItemGroupNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetById(1));
        }

        [Fact]
        public async Task Add_AddsNewItemGroup()
        {
            // Arrange
            var newItemGroup = new ItemGroupModel { name = "NewGroup" };

            // Act
            await _service.Add(newItemGroup);

            // Assert
            var itemGroup = await _db.ItemGroups.FirstOrDefaultAsync(x => x.name == "NewGroup");
            Assert.NotNull(itemGroup);
            Assert.Equal(newItemGroup.name, itemGroup.name);
        }

        [Fact]
        public async Task Update_UpdatesExistingItemGroup()
        {
            // Arrange
            var existingItemGroup = new ItemGroupModel { id = 1, name = "OldName" };
            await _db.ItemGroups.AddAsync(existingItemGroup);
            await _db.SaveChangesAsync();

            var updatedItemGroup = new ItemGroupModel { id = 1, name = "UpdatedName" };

            // Act
            var result = await _service.Update(1, updatedItemGroup);

            // Assert
            Assert.True(result);
            var itemGroup = await _db.ItemGroups.FindAsync(1);
            Assert.Equal(updatedItemGroup.name, itemGroup.name);
        }

        [Fact]
        public async Task Update_ReturnsFalse_WhenItemGroupNotFound()
        {
            // Act
            var result = await _service.Update(1, new ItemGroupModel());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Delete_DeletesExistingItemGroup()
        {
            // Arrange
            var existingItemGroup = new ItemGroupModel { id = 1, name = "GroupToDelete" };
            await _db.ItemGroups.AddAsync(existingItemGroup);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.Delete(1);

            // Assert
            Assert.True(result);
            var itemGroup = await _db.ItemGroups.FindAsync(1);
            Assert.Null(itemGroup);
            var archivedGroup = await _db.ArchivedItemGroups.FirstOrDefaultAsync();
            Assert.NotNull(archivedGroup);
            Assert.Equal(existingItemGroup.name, archivedGroup.name);
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenItemGroupNotFound()
        {
            // Act
            var result = await _service.Delete(1);

            // Assert
            Assert.False(result);
        }
    }
}
