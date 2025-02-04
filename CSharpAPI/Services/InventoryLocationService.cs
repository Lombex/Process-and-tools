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
        Task ReceiveShipment(int inventoryId, double amountReceived);
        Task PlaceOrder(string item_id, double amountOrdered);
        Task RemoveOrder(string itemId, int amount);
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
            return await _db.InventoryLocations.Include(il => il.Inventory).Include(il => il.Location).FirstOrDefaultAsync(il => il.Id == id);
        }

        public async Task<InventoryLocationModel> Create(InventoryLocationModel model)
        {
            if (model.InventoryId <= 0 || model.LocationId <= 0 || model.Amount < 0)
                throw new ArgumentException("Invalid inventory or location ID, or negative amount specified");

            var inventory = await _db.Inventors.FirstOrDefaultAsync(i => i.id == model.InventoryId);
            if (inventory == null)
                throw new ArgumentException($"Inventory with ID {model.InventoryId} not found");

            var location = await _db.Location.FirstOrDefaultAsync(l => l.id == model.LocationId);
            if (location == null)
                throw new ArgumentException($"Location with ID {model.LocationId} not found");

            var newRecord = new InventoryLocationModel
            {
                InventoryId = model.InventoryId,
                LocationId = model.LocationId,
                Amount = model.Amount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.InventoryLocations.Add(newRecord);
            await _db.SaveChangesAsync();

            await UpdateCalculatedFields(model.InventoryId);

            return await GetById(newRecord.Id); // Reload to include navigation properties
        }

        public async Task<InventoryLocationModel> Update(int id, InventoryLocationModel model)
        {
            var record = await _db.InventoryLocations
                .Include(il => il.Inventory)
                .Include(il => il.Location)
                .FirstOrDefaultAsync(il => il.Id == id);

            if (record == null)
                throw new Exception("Inventory location not found");

            record.Amount = model.Amount;
            record.UpdatedAt = DateTime.UtcNow;

            _db.InventoryLocations.Update(record);
            await _db.SaveChangesAsync();

            await UpdateCalculatedFields(model.InventoryId);

            return record;
        }

        public async Task Delete(int id)
        {
            var record = await GetById(id);
            _db.InventoryLocations.Remove(record);
            await _db.SaveChangesAsync();

            await UpdateCalculatedFields(record.InventoryId);
        }

        public async Task UpdateCalculatedFields(int inventoryId)
        {
            var inventory = await _db.Inventors.FindAsync(inventoryId);
            if (inventory == null) throw new Exception("Inventory not found");

            inventory.total_on_hand = 0;

            foreach (var location in inventory.locations) inventory.total_on_hand += location.amount;
            inventory.total_available = inventory.total_on_hand - inventory.total_ordered;

            _db.Inventors.Update(inventory);
            await _db.SaveChangesAsync();
        }

        public async Task ReceiveShipment(int inventoryId, double amountReceived)
        {
            var inventory = await _db.Inventors.FindAsync(inventoryId);
            if (inventory != null)
            {
                inventory.total_on_hand += (int)amountReceived;
                inventory.total_expected -= (int)amountReceived; // Adjust if shipment is part of expected inventory
                await _db.SaveChangesAsync();
            }
        }

        public async Task PlaceOrder(string itemId, double amountOrdered)
        {
            var inventory = await _db.Inventors.FirstOrDefaultAsync(i => i.item_id == itemId);
            if (inventory == null)
                throw new Exception($"Inventory not found for item ID {itemId}");

            inventory.total_allocated += (int)amountOrdered;
            inventory.total_available = inventory.total_on_hand - inventory.total_allocated;

            _db.Inventors.Update(inventory);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveOrder(string itemId, int amount)
        {
            var inventory = await _db.Inventors.FirstOrDefaultAsync(i => i.item_id == itemId);
            if (inventory == null)
                throw new Exception($"Inventory not found for item ID {itemId}");

            // Reduce allocated stock and adjust available stock
            inventory.total_allocated -= amount;
            if (inventory.total_allocated < 0) inventory.total_allocated = 0;

            inventory.total_available = inventory.total_on_hand - inventory.total_allocated;

            _db.Inventors.Update(inventory);
            await _db.SaveChangesAsync();
        }
    }
}