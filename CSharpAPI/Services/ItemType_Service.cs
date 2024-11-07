using CSharpAPI.Models;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface IItemTypeService
    {
        List<ItemType> GetAll();
        ItemType GetById(int id);
        void Add(ItemType itemType);
        bool Update(int id, ItemType itemType);
        bool Delete(int id);
    }

    public class ItemTypeService : IItemTypeService
    {
        private readonly string dataPath = "data/itemtypes.json";

        public List<ItemType> GetAll()
        {
            if (!File.Exists(dataPath)) return new List<ItemType>();
            return JsonConvert.DeserializeObject<List<ItemType>>(File.ReadAllText(dataPath)) ?? new List<ItemType>();
        }

        public ItemType GetById(int id)
        {
            var itemType = GetAll().FirstOrDefault(x => x.id == id);
            if (itemType == null) throw new Exception($"ItemType {id} not found");
            return itemType;
        }

        public void Add(ItemType itemType)
        {
            var items = GetAll();
            itemType.id = items.Count > 0 ? items.Max(x => x.id) + 1 : 1;
            itemType.created_at = DateTime.UtcNow;
            itemType.updated_at = DateTime.UtcNow;
            items.Add(itemType);
            File.WriteAllText(dataPath, JsonConvert.SerializeObject(items, Formatting.Indented));
        }

        public bool Update(int id, ItemType itemType)
        {
            var items = GetAll();
            var existing = items.FirstOrDefault(x => x.id == id);
            if (existing == null) return false;

            existing.name = itemType.name;
            existing.description = itemType.description;
            existing.updated_at = DateTime.UtcNow;

            File.WriteAllText(dataPath, JsonConvert.SerializeObject(items, Formatting.Indented));
            return true;
        }

        public bool Delete(int id)
        {
            var items = GetAll();
            var item = items.FirstOrDefault(x => x.id == id);
            if (item == null) return false;

            items.Remove(item);
            File.WriteAllText(dataPath, JsonConvert.SerializeObject(items, Formatting.Indented));
            return true;
        }
    }
}