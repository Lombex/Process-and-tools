using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services.V2
{
    public interface IInventoryLocationService
    {
        Task<List<InventoryLocationModel>> GetAll();
        Task<InventoryLocationModel> GetById(int id);
        Task<InventoryLocationModel> Create(InventoryLocationModel model);
        Task<InventoryLocationModel> Update(int id, InventoryLocationModel model);
        Task Delete(int id);
        Task UpdateCalculatedFields(int inventoryId);
    }

    public class InventoryLocationService : IInventoryLocationService
    {
        private readonly SQLiteDatabase _db;

        public InventoryLocationService(SQLiteDatabase db)
        {
            _db = db;
        }

        public async Task<List<InventoryLocationModel>> GetAll()
        {
            return await _db.InventoryLocations
                .Include(il => il.Inventory)
                .Include(il => il.Location)
                .ToListAsync();
        }

        public async Task<InventoryLocationModel> GetById(int id)
        {
            var inventoryLocation = await _db.InventoryLocations
                .Include(il => il.Inventory)
                .Include(il => il.Location)
                .FirstOrDefaultAsync(il => il.Id == id);

            if (inventoryLocation == null)
                throw new Exception($"InventoryLocation with id {id} not found");

            return inventoryLocation;
        }

        public async Task<InventoryLocationModel> Create(InventoryLocationModel model)
        {
            // Validate required fields
            if (model.InventoryId <= 0)
                throw new ArgumentException("Invalid inventory ID");

            if (model.LocationId <= 0)
                throw new ArgumentException("Invalid location ID");

            if (model.Amount < 0)
                throw new ArgumentException("Amount cannot be negative");

            // Check if inventory exists
            var inventory = await _db.Inventors
                .FirstOrDefaultAsync(i => i.id == model.InventoryId);
            if (inventory == null)
                throw new ArgumentException($"Inventory with ID {model.InventoryId} not found");

            // Check if location exists
            var location = await _db.Location
                .FirstOrDefaultAsync(l => l.id == model.LocationId);
            if (location == null)
                throw new ArgumentException($"Location with ID {model.LocationId} not found");

            // Check if this inventory-location combination already exists
            var existing = await _db.InventoryLocations
                .FirstOrDefaultAsync(il => il.InventoryId == model.InventoryId && il.LocationId == model.LocationId);
            if (existing != null)
                throw new ArgumentException($"An inventory location already exists for inventory {model.InventoryId} at location {model.LocationId}");

            // Create new inventory location without navigation properties
            var newInventoryLocation = new InventoryLocationModel
            {
                InventoryId = model.InventoryId,
                LocationId = model.LocationId,
                Amount = model.Amount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Entry(newInventoryLocation).State = EntityState.Added;
            await _db.SaveChangesAsync();

            // Update calculated fields
            await UpdateCalculatedFields(model.InventoryId);

            // Load the complete model with relationships for return
            return await _db.InventoryLocations
                .Include(il => il.Inventory)
                .Include(il => il.Location)
                .FirstAsync(il => il.Id == newInventoryLocation.Id);
        }

        public async Task<InventoryLocationModel> Update(int id, InventoryLocationModel model)
        {
            var existing = await _db.InventoryLocations
                .Include(il => il.Inventory)
                .Include(il => il.Location)
                .FirstOrDefaultAsync(il => il.Id == id);

            if (existing == null)
                throw new Exception($"InventoryLocation with id {id} not found");

            // Verify new inventory exists if it's being changed
            if (model.InventoryId != existing.InventoryId)
            {
                var newInventory = await _db.Inventors.FindAsync(model.InventoryId);
                if (newInventory == null)
                    throw new ArgumentException($"Inventory with ID {model.InventoryId} not found");

                // Update inventory reference
                existing.InventoryId = model.InventoryId;
                existing.Inventory = newInventory;
            }

            // Verify new location exists if it's being changed
            if (model.LocationId != existing.LocationId)
            {
                var newLocation = await _db.Location.FindAsync(model.LocationId);
                if (newLocation == null)
                    throw new ArgumentException($"Location with ID {model.LocationId} not found");

                // Update location reference
                existing.LocationId = model.LocationId;
                existing.Location = newLocation;
            }

            // Check if this combination already exists (only if location or inventory changed)
            if (model.LocationId != existing.LocationId || model.InventoryId != existing.InventoryId)
            {
                var duplicate = await _db.InventoryLocations
                    .FirstOrDefaultAsync(il =>
                        il.Id != id &&
                        il.InventoryId == model.InventoryId &&
                        il.LocationId == model.LocationId);

                if (duplicate != null)
                    throw new ArgumentException($"An inventory location already exists for inventory {model.InventoryId} at location {model.LocationId}");
            }

            // Update basic properties
            existing.Amount = model.Amount;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Update calculated fields for both old and new inventory if changed
            if (model.InventoryId != existing.InventoryId)
            {
                await UpdateCalculatedFields(model.InventoryId);
                await UpdateCalculatedFields(existing.InventoryId);
            }
            else
            {
                await UpdateCalculatedFields(existing.InventoryId);
            }

            return existing;
        }

        public async Task Delete(int id)
        {
            var inventoryLocation = await GetById(id);
            var inventoryId = inventoryLocation.InventoryId;

            _db.InventoryLocations.Remove(inventoryLocation);
            await _db.SaveChangesAsync();

            await UpdateCalculatedFields(inventoryId);
        }

        public async Task UpdateCalculatedFields(int inventoryId)
        {
            var inventory = await _db.Inventors.FindAsync(inventoryId);
            if (inventory == null)
                throw new Exception($"Inventory with id {inventoryId} not found");

            // Calculate total_on_hand based on location amounts
            var totalOnHand = await _db.InventoryLocations.Where(il => il.InventoryId == inventoryId).SumAsync(il => (int)il.Amount);

            inventory.total_on_hand = totalOnHand;
            inventory.total_expected = totalOnHand + inventory.total_ordered;
            inventory.total_available = totalOnHand - inventory.total_allocated;

            _db.Inventors.Update(inventory);
            await _db.SaveChangesAsync();
        }
    }
}