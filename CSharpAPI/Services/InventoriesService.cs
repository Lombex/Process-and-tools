using CSharpAPI.Models;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{   
     public interface IInventoriesService
    {
        IEnumerable<InventoriesModel> GetAllInventories();
        InventoriesModel GetInventoryById(int id);
        void CreateInventory(InventoriesModel inventory);
        bool UpdateInventory(int id, InventoriesModel inventory);
        bool DeleteInventory(int id);
        IEnumerable<InventoriesModel> GetInventoriesByItemId(string itemId);
        IEnumerable<InventoriesModel> GetInventoriesByLocation(int locationId);
    }

    public class InventoriesService : IInventoriesService
    {
        private List<InventoriesModel> _inventories;
        private int _nextId = 1;

        public InventoriesService()
        {
            _inventories = new List<InventoriesModel>
            {
                new InventoriesModel
                {
                    id = 0,
                    item_id = "ITEM001",
                    description = "Default Inventory",
                    locations = new List<int> { 0 },
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };
            _nextId = 1;
        }

        public IEnumerable<InventoriesModel> GetAllInventories() => _inventories;

        public InventoriesModel GetInventoryById(int id)
        {
            var inventory = _inventories.FirstOrDefault(x => x.id == id);
            if (inventory == null)
            {
                throw new Exception($"Inventory with id {id} not found");
            }
            return inventory;
        }

        public void CreateInventory(InventoriesModel inventory)
        {
            inventory.id = _nextId++;
            inventory.created_at = DateTime.UtcNow;
            inventory.updated_at = DateTime.UtcNow;
            _inventories.Add(inventory);
        }

        public bool UpdateInventory(int id, InventoriesModel inventory)
        {
            var existingInventory = _inventories.FirstOrDefault(x => x.id == id);
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

            return true;
        }

        public bool DeleteInventory(int id)
        {
            var inventory = _inventories.FirstOrDefault(x => x.id == id);
            if (inventory == null) return false;

            _inventories.Remove(inventory);
            return true;
        }

        public IEnumerable<InventoriesModel> GetInventoriesByItemId(string itemId)
        {
            return _inventories.Where(i => i.item_id == itemId);
        }

        public IEnumerable<InventoriesModel> GetInventoriesByLocation(int locationId)
        {
            return _inventories.Where(i => i.locations != null && i.locations.Contains(locationId));
        }
    }
}