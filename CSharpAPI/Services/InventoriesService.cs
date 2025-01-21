using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Service
{   
     public interface IInventoriesService
    {
        Task<List<InventorieModel>> GetAllInventories();
        Task<InventorieModel> GetInventoryById(int id);
        Task AddInventory(InventorieModel inventory);
        Task<bool> UpdateInventory(int id, InventorieModel inventory);
        Task<bool> DeleteInventory(int id);
        Task<List<InventorieModel>> GetInventoriesByItemId(string itemId);
        Task<List<InventorieModel>> GetInventoriesByLocation(int locationId);
    }

    public class InventoriesService : IInventoriesService
    {
        private readonly SQLiteDatabase _Db;

        public InventoriesService(SQLiteDatabase sQLite)
        {
            _Db = sQLite;
        }

        public async Task<List<InventorieModel>> GetAllInventories()
        {
            return await _Db.Inventors.ToListAsync();
        }

        public async Task<InventorieModel> GetInventoryById(int id)
        {
            var inventory = await _Db.Inventors.FirstOrDefaultAsync(x => x.id == id);
            if (inventory == null)
            {
                throw new Exception($"Inventory with id {id} not found");
            }
            return inventory;
        }

        public async Task AddInventory(InventorieModel inventory)
        {
            inventory.created_at = DateTime.UtcNow;
            inventory.updated_at = DateTime.UtcNow;

            await _Db.Inventors.AddAsync(inventory);
            await _Db.SaveChangesAsync();
        }

        public async Task<bool> UpdateInventory(int id, InventorieModel inventory)
        {
            var existingInventory = await _Db.Inventors.FirstOrDefaultAsync(x => x.id == id);
            if (existingInventory == null) return false;

            existingInventory.item_id = inventory.item_id;
            existingInventory.description = inventory.description;
            existingInventory.item_reference = inventory.item_reference;
            existingInventory.locations = inventory.locations;
            existingInventory.total_on_hand = inventory.total_on_hand;
            existingInventory.total_expected = inventory.total_expected;
            existingInventory.total_ordered = inventory.total_ordered;
            existingInventory.total_allocated = inventory.total_allocated;
            existingInventory.total_available = inventory.total_available;
            existingInventory.updated_at = DateTime.UtcNow;

            _Db.Inventors.Update(existingInventory);
            await _Db.SaveChangesAsync();

            return true;
        }

       public async Task<bool> DeleteInventory(int id)
        {
            var inventory = await _Db.Inventors.FirstOrDefaultAsync(x => x.id == id);
            if (inventory == null) return false;

            // Log de gegevens van de inventaris
            Console.WriteLine($"Archiving Inventory: {inventory.id}, {inventory.description}");

            var archivedInventory = new ArchivedInventorieModel
            {
                id = inventory.id,
                item_id = inventory.item_id,
                description = inventory.description,
                item_reference = inventory.item_reference,
                locations = inventory.locations,
                total_on_hand = inventory.total_on_hand,
                total_expected = inventory.total_expected,
                total_ordered = inventory.total_ordered,
                total_allocated = inventory.total_allocated,
                total_available = inventory.total_available,
                created_at = inventory.created_at,
                updated_at = inventory.updated_at,
                archived_at = DateTime.UtcNow
            };

            // Controleer of de archiefdata correct is gevuld
            Console.WriteLine($"Archived Inventory: {archivedInventory.id}, {archivedInventory.description}");

            await _Db.ArchivedInventories.AddAsync(archivedInventory);

            // Verwijder het originele record
            _Db.Inventors.Remove(inventory);

            // Sla wijzigingen op
            await _Db.SaveChangesAsync();

            return true;
        }

        public async Task<List<InventorieModel>> GetInventoriesByItemId(string itemId)
        {
            return await _Db.Inventors.Where(i => i.item_id == itemId).ToListAsync();
        }

        public async Task<List<InventorieModel>> GetInventoriesByLocation(int locationId)
        {
            return await _Db.Inventors.Where(i => i.locations.Contains(locationId)).ToListAsync();
        }
    }
}