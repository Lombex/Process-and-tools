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

        public List<Warehouse> GetAllWarehouses()
        {
            try
            {
                if (!File.Exists(dataFolder))
                    return new List<Warehouse>();

                var jsonContent = File.ReadAllText(dataFolder);
                return JsonConvert.DeserializeObject<List<Warehouse>>(jsonContent) ?? new List<Warehouse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return new List<Warehouse>();
            }
        }

        public Warehouse GetWarehouseById(int id)
        {
            return GetAllWarehouses().FirstOrDefault(w => w.Id == id);
        }

        public void AddWarehouse(Warehouse newWarehouse)
        {
            var warehouses = GetAllWarehouses();
            newWarehouse.Id = warehouses.Count > 0 ? warehouses.Max(w => w.Id) + 1 : 1; // Auto increment ID
            warehouses.Add(newWarehouse);
            SaveWarehouses(warehouses);
        }

        public bool UpdateWarehouse(int id, Warehouse updatedWarehouse)
        {
            var warehouses = GetAllWarehouses();
            var existingWarehouse = warehouses.FirstOrDefault(w => w.Id == id);

            if (existingWarehouse == null)
                return false;

            existingWarehouse.Code = updatedWarehouse.Code;
            existingWarehouse.Name = updatedWarehouse.Name;
            existingWarehouse.Address = updatedWarehouse.Address;
            existingWarehouse.Zip = updatedWarehouse.Zip;
            existingWarehouse.City = updatedWarehouse.City;
            existingWarehouse.Province = updatedWarehouse.Province;
            existingWarehouse.Country = updatedWarehouse.Country;
            existingWarehouse.Contact = updatedWarehouse.Contact;
            existingWarehouse.CreatedAt = updatedWarehouse.CreatedAt;
            existingWarehouse.UpdatedAt = updatedWarehouse.UpdatedAt;

            SaveWarehouses(warehouses);
            return true;
        }

        public bool DeleteWarehouse(int id)
        {
            var warehouses = GetAllWarehouses();
            var warehouseToDelete = warehouses.FirstOrDefault(w => w.Id == id);

            if (warehouseToDelete == null)
                return false;

            warehouses.Remove(warehouseToDelete);
            SaveWarehouses(warehouses);
            return true;
        }

        private void SaveWarehouses(List<Warehouse> warehouses)
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
        List<Warehouse> GetAllWarehouses();
        Warehouse GetWarehouseById(int id);
        void AddWarehouse(Warehouse newWarehouse);
        bool UpdateWarehouse(int id, Warehouse updatedWarehouse);
        bool DeleteWarehouse(int id);
    }
}
