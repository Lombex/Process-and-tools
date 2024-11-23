using Newtonsoft.Json;
using CSharpAPI.Models;

namespace CSharpAPI.Service
{
    public class WarehouseService : IWarehouseService
    {
        private readonly string dataFolder;

        public WarehouseService(string jsonFilePath = "data/warehouses.json")
        {
            dataFolder = jsonFilePath;
        }

        public List<WarehouseModel> GetAllWarehouses()
        {
            try
            {
                if (!File.Exists(dataFolder))
                    return new List<WarehouseModel>();

                var jsonContent = File.ReadAllText(dataFolder);
                return JsonConvert.DeserializeObject<List<WarehouseModel>>(jsonContent) ?? new List<WarehouseModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return new List<WarehouseModel>();
            }
        }

        public WarehouseModel GetWarehouseById(int id) => GetAllWarehouses().FirstOrDefault(w => w.id == id);
            
        public void AddWarehouse(WarehouseModel newWarehouse)
        {
            var warehouses = GetAllWarehouses();
            newWarehouse.id = warehouses.Count > 0 ? warehouses.Max(w => w.id) + 1 : 1; // Auto increment ID
            warehouses.Add(newWarehouse);
            SaveWarehouses(warehouses);
        }

        public bool UpdateWarehouse(int id, WarehouseModel updatedWarehouse)
        {
            var warehouses = GetAllWarehouses();
            var existingWarehouse = warehouses.FirstOrDefault(w => w.id == id);

            if (existingWarehouse == null)
                return false;

            existingWarehouse.code = updatedWarehouse.code;
            existingWarehouse.name = updatedWarehouse.name;
            existingWarehouse.address = updatedWarehouse.address;
            existingWarehouse.zip = updatedWarehouse.zip;
            existingWarehouse.city = updatedWarehouse.city;
            existingWarehouse.province = updatedWarehouse.province;
            existingWarehouse.country = updatedWarehouse.country;
            existingWarehouse.contact = updatedWarehouse.contact;
            existingWarehouse.created_at = updatedWarehouse.created_at;
            existingWarehouse.updated_at = updatedWarehouse.updated_at;

            SaveWarehouses(warehouses);
            return true;
        }

        public bool DeleteWarehouse(int id)
        {
            var warehouses = GetAllWarehouses();
            var warehouseToDelete = warehouses.FirstOrDefault(w => w.id == id);

            if (warehouseToDelete == null)
                return false;

            warehouses.Remove(warehouseToDelete);
            SaveWarehouses(warehouses);
            return true;
        }

        private void SaveWarehouses(List<WarehouseModel> warehouses)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(warehouses, Formatting.Indented);
                File.WriteAllText(dataFolder, jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file: {ex.Message}");
            }
        }
    }
    public interface IWarehouseService
    {
        List<WarehouseModel> GetAllWarehouses();
        WarehouseModel GetWarehouseById(int id);
        void AddWarehouse(WarehouseModel newWarehouse);
        bool UpdateWarehouse(int id, WarehouseModel updatedWarehouse);
        bool DeleteWarehouse(int id);
    }
}
