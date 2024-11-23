using CSharpAPI.Models;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface IItemTypeService
    {
        List<ItemTypeModel> GetAll();
        ItemTypeModel GetById(int id);
        void Add(ItemTypeModel itemType);
        bool Update(int id, ItemTypeModel itemType);
        bool Delete(int id);
    }

    public class ItemTypeService : IItemTypeService
    {
        private readonly string dataPath = "data/itemtypes.json";

        public List<ItemTypeModel> GetAll()
        {
            if (!File.Exists(dataPath)) return new List<ItemTypeModel>();
            return JsonConvert.DeserializeObject<List<ItemTypeModel>>(File.ReadAllText(dataPath)) ?? new List<ItemTypeModel>();
        }

        public ItemTypeModel GetById(int id)
        {
            var itemType = GetAll().FirstOrDefault(x => x.id == id);
            if (itemType == null) throw new Exception($"ItemType {id} not found");
            return itemType;
        }

        public void Add(ItemTypeModel itemType)
        {
            var items = GetAll();
            itemType.id = items.Count > 0 ? items.Max(x => x.id) + 1 : 1;
            itemType.created_at = DateTime.UtcNow;
            itemType.updated_at = DateTime.UtcNow;
            items.Add(itemType);
            File.WriteAllText(dataPath, JsonConvert.SerializeObject(items, Formatting.Indented));
        }

        public bool Update(int id, ItemTypeModel itemType)
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