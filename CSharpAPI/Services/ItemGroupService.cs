using CSharpAPI.Models;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface IItemGroupService
    {
        List<ItemGroupModel> GetAll();
        ItemGroupModel GetById(int id);
        void Add(ItemGroupModel ItemGroupModel);
        bool Update(int id, ItemGroupModel ItemGroupModel);
        bool Delete(int id);
    }

    public class ItemGroupService : IItemGroupService
    {
        private readonly string dataPath = "data/ItemGroupModels.json";

        public List<ItemGroupModel> GetAll()
        {
            if (!File.Exists(dataPath)) return new List<ItemGroupModel>();
            return JsonConvert.DeserializeObject<List<ItemGroupModel>>(File.ReadAllText(dataPath)) ?? new List<ItemGroupModel>();
        }

        public ItemGroupModel GetById(int id)
        {
            var itemgroup = GetAll().FirstOrDefault(x => x.id == id);
            if (itemgroup == null) throw new Exception($"ItemGroup {id} not found");
            return itemgroup;
        }

        public void Add(ItemGroupModel itemGroup)
        {
            var items = GetAll();
            itemGroup.id = items.Count > 0 ? items.Max(x => x.id) + 1 : 1;
            itemGroup.created_at = DateTime.UtcNow;
            itemGroup.updated_at = DateTime.UtcNow;
            items.Add(itemGroup);
            File.WriteAllText(dataPath, JsonConvert.SerializeObject(items, Formatting.Indented));
        }

        public bool Update(int id, ItemGroupModel itemGroup)
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