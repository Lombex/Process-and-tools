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
        Task ReceiveShipment(string itemId, double amountReceived);
        Task PlaceOrder(string itemId, double amountOrdered);
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
            var location = await _db.InventoryLocations
                .Include(il => il.Inventory)
                .Include(il => il.Location)
                .FirstOrDefaultAsync(il => il.Id == id);

            if (location == null)
                throw new Exception($"InventoryLocation with id {id} not found");

            return location;
        }

        public async Task<InventoryLocationModel> Create(InventoryLocationModel model)
        {
            if (model.InventoryId <= 0 || model.LocationId <= 0 || model.Amount < 0)
                throw new ArgumentException("Invalid inventory or location ID, or negative amount");

            var inventory = await _db.Inventors.FindAsync(model.InventoryId);
            if (inventory == null)
                throw new ArgumentException($"Inventory {model.InventoryId} not found");

            var location = await _db.Location.FindAsync(model.LocationId);
            if (location == null)
                throw new ArgumentException($"Location {model.LocationId} not found");

            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            _db.InventoryLocations.Add(model);
            await _db.SaveChangesAsync();
            await UpdateCalculatedFields(model.InventoryId);

            return await GetById(model.Id);
        }

        public async Task<InventoryLocationModel> Update(int id, InventoryLocationModel model)
        {
            var existing = await GetById(id);
            if (existing == null)
                throw new Exception("InventoryLocation not found");

            existing.Amount = model.Amount;
            existing.UpdatedAt = DateTime.UtcNow;

            _db.InventoryLocations.Update(existing);
            await _db.SaveChangesAsync();
            await UpdateCalculatedFields(existing.InventoryId);

            return existing;
        }

        public async Task Delete(int id)
        {
            var location = await GetById(id);
            if (location == null)
                throw new Exception("InventoryLocation not found");

            _db.InventoryLocations.Remove(location);
            await _db.SaveChangesAsync();
            await UpdateCalculatedFields(location.InventoryId);
        }

        public async Task UpdateCalculatedFields(int inventoryId)
        {
            var inventory = await _db.Inventors
                .Include(i => i.locations)
                .FirstOrDefaultAsync(i => i.id == inventoryId);

            if (inventory == null) 
                throw new Exception("Inventory not found");

            // Calculate total_on_hand from locations
            inventory.total_on_hand = inventory.locations?.Sum(loc => loc.amount) ?? 0;

            // Calculate total_expected from incoming shipments
            var incomingShipments = await _db.Shipment
                .Include(s => s.items)
                .Where(s => s.shipment_status == "I")
                .ToListAsync();

            inventory.total_expected = incomingShipments
                .SelectMany(s => s.items)
                .Where(i => i.item_id == inventory.item_id)
                .Sum(i => i.amount);

            // Calculate total_ordered from pending orders
            var pendingOrders = await _db.Order
                .Include(o => o.items)
                .Where(o => o.order_status == "Pending")
                .ToListAsync();

            inventory.total_ordered = pendingOrders
                .SelectMany(o => o.items)
                .Where(i => i.item_id == inventory.item_id)
                .Sum(i => i.amount);

            // Calculate total_allocated from orders with any associated shipments
            var shipments = await _db.Shipment
                .Include(s => s.items)
                .Where(s => s.shipment_status != "Completed" && s.shipment_status != "Cancelled")
                .ToListAsync();

            var orders = await _db.Order
                .Include(o => o.items)
                .Where(o => o.order_status != "Completed" && o.order_status != "Cancelled")
                .ToListAsync();

            var allocatedItems = orders
                .Where(o => o.shipment_id.HasValue)
                .Join(shipments,
                    o => o.shipment_id,
                    s => s.id,
                    (o, s) => o.items)
                .SelectMany(items => items)
                .Where(i => i.item_id == inventory.item_id);

            inventory.total_allocated = allocatedItems.Sum(i => i.amount);

            // Calculate total_available
            inventory.total_available = (inventory.total_on_hand + inventory.total_expected) 
                                     - (inventory.total_ordered + inventory.total_allocated);

            _db.Inventors.Update(inventory);
            await _db.SaveChangesAsync();
        }

        public async Task ReceiveShipment(string itemId, double amountReceived)
        {
            var inventory = await _db.Inventors.FirstOrDefaultAsync(i => i.item_id == itemId);
            if (inventory == null)
                throw new Exception($"Inventory not found for item {itemId}");

            inventory.total_on_hand += (int)amountReceived;
            inventory.total_expected -= (int)amountReceived;

            _db.Inventors.Update(inventory);
            await _db.SaveChangesAsync();
            await UpdateCalculatedFields(inventory.id);
        }

        public async Task PlaceOrder(string itemId, double amountOrdered)
        {
            var inventory = await _db.Inventors.FirstOrDefaultAsync(i => i.item_id == itemId);
            if (inventory == null)
                throw new Exception($"Inventory not found for item {itemId}");

            inventory.total_ordered += (int)amountOrdered;
            inventory.total_available = (inventory.total_on_hand + inventory.total_expected) 
                                     - (inventory.total_ordered + inventory.total_allocated);

            _db.Inventors.Update(inventory);
            await _db.SaveChangesAsync();
            await UpdateCalculatedFields(inventory.id);
        }

        public async Task RemoveOrder(string itemId, int amount)
        {
            var inventory = await _db.Inventors.FirstOrDefaultAsync(i => i.item_id == itemId);
            if (inventory == null)
                throw new Exception($"Inventory not found for item {itemId}");

            inventory.total_ordered = Math.Max(0, inventory.total_ordered - amount);
            inventory.total_available = (inventory.total_on_hand + inventory.total_expected) 
                                     - (inventory.total_ordered + inventory.total_allocated);

            _db.Inventors.Update(inventory);
            await _db.SaveChangesAsync();
            await UpdateCalculatedFields(inventory.id);
        }
    }
}