using Newtonsoft.Json;
using CSharpAPI.Models;

namespace CSharpAPI.Service
{
    public class InventoriesService : IInventoriesService
    {
        private readonly string dataFolder;

        public InventoriesService(string jsonFilePath = "data/inventories.json")
        {
            dataFolder = jsonFilePath;
        }

        public List<InventoriesModel> GetAllInventories()
        {
            try
            {
                if (!File.Exists(dataFolder))
                    return new List<InventoriesModel>();

                var jsonContent = File.ReadAllText(dataFolder);
                return JsonConvert.DeserializeObject<List<InventoriesModel>>(jsonContent) ?? new List<InventoriesModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return new List<InventoriesModel>();
            }
        }

        public InventoriesModel GetInventoryById(int id) => GetAllInventories().FirstOrDefault(i => i.id == id);

        public void AddInventory(InventoriesModel newInventory)
        {
            var inventories = GetAllInventories();
            newInventory.id = inventories.Count > 0 ? inventories.Max(i => i.id) + 1 : 1; // Auto increment ID
            inventories.Add(newInventory);
            SaveInventories(inventories);
        }

        public bool UpdateInventory(int id, InventoriesModel updatedInventory)
        {
            var inventories = GetAllInventories();
            var existingInventory = inventories.FirstOrDefault(i => i.id == id);

            if (existingInventory == null)
                return false;

            existingInventory.item_id = updatedInventory.item_id;
            existingInventory.description = updatedInventory.description;
            existingInventory.item_reference = updatedInventory.item_reference;
            existingInventory.locations = updatedInventory.locations;
            existingInventory.total_on_hand = updatedInventory.total_on_hand;
            existingInventory.total_expected = updatedInventory.total_expected;
            existingInventory.total_ordered = updatedInventory.total_ordered;
            existingInventory.total_allocated = updatedInventory.total_allocated;
            existingInventory.total_available = updatedInventory.total_available;
            existingInventory.created_at = updatedInventory.created_at;
            existingInventory.updated_at = updatedInventory.updated_at;

            SaveInventories(inventories);
            return true;
        }

        public bool DeleteInventory(int id)
        {
            var inventories = GetAllInventories();
            var inventoryToDelete = inventories.FirstOrDefault(i => i.id == id);

            if (inventoryToDelete == null)
                return false;

            inventories.Remove(inventoryToDelete);
            SaveInventories(inventories);
            return true;
        }

        private void SaveInventories(List<InventoriesModel> inventories)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(inventories, Formatting.Indented);
                File.WriteAllText(dataFolder, jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file: {ex.Message}");
            }
        }
    }

    public interface IInventoriesService
    {
        List<InventoriesModel> GetAllInventories();
        InventoriesModel GetInventoryById(int id);
        void AddInventory(InventoriesModel newInventory);
        bool UpdateInventory(int id, InventoriesModel updatedInventory);
        bool DeleteInventory(int id);
    }
}
