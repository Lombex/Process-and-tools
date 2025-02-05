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
            return await _db.InventoryLocations
                .Include(il => il.Inventory)
                .Include(il => il.Location)
                .FirstOrDefaultAsync(il => il.Id == id);
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

            return await GetById(newRecord.Id); 
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
            var inventory = await _db.Inventors
                .Include(i => i.locations)
                .FirstOrDefaultAsync(i => i.id == inventoryId);

            if (inventory == null) throw new Exception("Inventory not found");

            inventory.total_on_hand = inventory.locations?.Sum(loc => loc.amount) ?? 0;

            // ✅ Count incoming shipments (`shipment_status = "I"`)
            var shipments = await _db.Shipment
                .Include(s => s.items)
                .Where(s => s.shipment_status == "I")
                .ToListAsync();

            inventory.total_expected = shipments
                .SelectMany(s => s.items)
                .Where(i => i.item_id == inventory.item_id)
                .Sum(i => i.amount);

            // ✅ Update `total_ordered` from `PENDING` orders
            var pendingOrders = await _db.Order
                .Include(o => o.items)
                .Where(o => o.order_status == "Pending")
                .ToListAsync();

            inventory.total_ordered = pendingOrders
                .SelectMany(o => o.items)
                .Where(i => i.item_id == inventory.item_id)
                .Sum(i => i.amount);

            // ✅ Update `total_allocated` for `DELIVERED` orders with `TRANSIT` shipments
            var allocatedOrders = await _db.Order
                .Where(o => o.order_status == "Delivered")
                .Join(_db.Shipment,
                    o => o.id,
                    s => s.order_id,
                    (o, s) => new { Order = o, Shipment = s })
                .Where(os => os.Shipment.shipment_status == "Transit")
                .ToListAsync();

            inventory.total_allocated = allocatedOrders
                .SelectMany(os => os.Order.items)
                .Where(i => i.item_id == inventory.item_id)
                .Sum(i => i.amount);

            // ✅ Calculate `total_available`
            inventory.total_available = (inventory.total_on_hand + inventory.total_expected) - (inventory.total_ordered + inventory.total_allocated);

            _db.Inventors.Update(inventory);
            await _db.SaveChangesAsync();
        }

        public async Task ReceiveShipment(int inventoryId, double amountReceived)
        {
            var inventory = await _db.Inventors.FindAsync(inventoryId);
            if (inventory != null)
            {
                inventory.total_on_hand += (int)amountReceived;
                inventory.total_expected -= (int)amountReceived;
                await _db.SaveChangesAsync();
            }
        }

        public async Task PlaceOrder(string itemId, double amountOrdered)
        {
            var inventory = await _db.Inventors.FirstOrDefaultAsync(i => i.item_id == itemId);
            if (inventory == null)
                throw new Exception($"Inventory not found for item ID {itemId}");

            await UpdateCalculatedFields(inventory.id);

            _db.Inventors.Update(inventory);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveOrder(string itemId, int amount)
        {
            var inventory = await _db.Inventors.FirstOrDefaultAsync(i => i.item_id == itemId);
            if (inventory == null)
                throw new Exception($"Inventory not found for item ID {itemId}");

            inventory.total_allocated = Math.Max(0, inventory.total_allocated - amount);
            inventory.total_ordered = Math.Max(0, inventory.total_ordered - amount);

            inventory.total_available = (inventory.total_on_hand + inventory.total_expected) - (inventory.total_ordered + inventory.total_allocated);

            _db.Inventors.Update(inventory);
            await _db.SaveChangesAsync();
        }
    }
}
