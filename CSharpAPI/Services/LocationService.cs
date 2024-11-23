using CSharpAPI.Models;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface ILocationService
    {
        List<LocationModel> GetAll();
        LocationModel GetById(int id);
        void Add(LocationModel location);
        bool Update(int id, LocationModel location);
        bool Delete(int id);
        List<LocationModel> GetByWarehouseId(int warehouseId);
    }

    public class LocationService : ILocationService
    {
        private readonly string dataPath = "data/locations.json";
        private static readonly List<LocationModel> _testData = new()
        {
            new LocationModel
            {
                id = 1,
                warehouse_id = 1,
                code = "LOC001",
                name = "Warehouse A Section 1",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            },
            new LocationModel
            {
                id = 2,
                warehouse_id = 1,
                code = "LOC002",
                name = "Warehouse A Section 2",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            },
            new LocationModel
            {
                id = 3,
                warehouse_id = 2,
                code = "LOC003",
                name = "Warehouse B Section 1",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            }
        };

        public List<LocationModel> GetAll()
        {
            if (!File.Exists(dataPath)) 
                return _testData;

            var jsonContent = File.ReadAllText(dataPath);
            return JsonConvert.DeserializeObject<List<LocationModel>>(jsonContent) ?? _testData;
        }

        public LocationModel GetById(int id)
        {
            var location = GetAll().FirstOrDefault(x => x.id == id);
            if (location == null) throw new Exception($"Location {id} not found");
            return location;
        }

        public List<LocationModel> GetByWarehouseId(int warehouseId)
        {
            return GetAll().Where(x => x.warehouse_id == warehouseId).ToList();
        }

        public void Add(LocationModel location)
        {
            var items = GetAll();
            location.id = items.Count > 0 ? items.Max(x => x.id) + 1 : 1;
            location.created_at = DateTime.UtcNow;
            location.updated_at = DateTime.UtcNow;
            items.Add(location);
            SaveToFile(items);
        }

        public bool Update(int id, LocationModel location)
        {
            var items = GetAll();
            var existing = items.FirstOrDefault(x => x.id == id);
            if (existing == null) return false;

            existing.warehouse_id = location.warehouse_id;
            existing.code = location.code;
            existing.name = location.name;
            existing.updated_at = DateTime.UtcNow;

            SaveToFile(items);
            return true;
        }

        public bool Delete(int id)
        {
            var items = GetAll();
            var item = items.FirstOrDefault(x => x.id == id);
            if (item == null) return false;

            items.Remove(item);
            SaveToFile(items);
            return true;
        }

        private void SaveToFile(List<LocationModel> items)
        {
            var jsonContent = JsonConvert.SerializeObject(items, Formatting.Indented);
            File.WriteAllText(dataPath, jsonContent);
        }
    }
}