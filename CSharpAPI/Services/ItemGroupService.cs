using CSharpAPI.Models;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface IItemGroupService
    {
        List<ItemGroup> GetAll();
        ItemGroup GetById(int id);
        void Add(ItemGroup itemGroup);
        bool Update(int id, ItemGroup itemGroup);
        bool Delete(int id);
    }

    public class ItemGroupService : IItemGroupService
    {
        private readonly string dataPath = "data/itemgroups.json";

        public List<ItemGroup> GetAll()
        {
            if (!File.Exists(dataPath)) return new List<ItemGroup>();
            return JsonConvert.DeserializeObject<List<ItemGroup>>(File.ReadAllText(dataPath)) ?? new List<ItemGroup>();
        }

        public ItemGroup GetById(int id)
        {
            var itemGroup = GetAll().FirstOrDefault(x => x.id == id);
            if (itemGroup == null) throw new Exception($"ItemGroup {id} not found");
            return itemGroup;
        }

        public void Add(ItemGroup itemGroup)
        {
            var items = GetAll();
            itemGroup.id = items.Count > 0 ? items.Max(x => x.id) + 1 : 1;
            itemGroup.created_at = DateTime.UtcNow;
            itemGroup.updated_at = DateTime.UtcNow;
            items.Add(itemGroup);
            File.WriteAllText(dataPath, JsonConvert.SerializeObject(items, Formatting.Indented));
        }

        public bool Update(int id, ItemGroup itemGroup)
        {
            var items = GetAll();
            var existing = items.FirstOrDefault(x => x.id == id);
            if (existing == null) return false;

            existing.name = itemGroup.name;
            existing.description = itemGroup.description;
            existing.itemtype_id = itemGroup.itemtype_id;
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