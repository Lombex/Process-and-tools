using CSharpAPI.Models;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public class ItemsService : IItemsService
    {
        private readonly string dataFolder;

        public ItemsService(string jsonFilePath = "data/items.json")
        {
            dataFolder = jsonFilePath;
        }

        public List<ItemsModel> GetAllItems()
        {
            if (!File.Exists(dataFolder))
                return new List<ItemsModel>();

            var jsonContent = File.ReadAllText(dataFolder);
            return JsonConvert.DeserializeObject<List<ItemsModel>>(jsonContent) ?? new List<ItemsModel>();
        }

        public ItemsModel GetItemById(string uid)
        {
            return GetAllItems().FirstOrDefault(i => i.uid == uid);
        }

        public IEnumerable<ItemsModel> GetItemsByItemLine(int itemLineId)
        {
            return GetAllItems().Where(i => i.item_line == itemLineId);
        }

        public IEnumerable<ItemsModel> GetItemsByItemGroup(int itemGroupId)
        {
            return GetAllItems().Where(i => i.item_group == itemGroupId);
        }

        public IEnumerable<ItemsModel> GetItemsByItemType(int itemTypeId)
        {
            return GetAllItems().Where(i => i.item_type == itemTypeId);
        }

        public IEnumerable<ItemsModel> GetItemsBySupplierId(int supplierId)
        {
            return GetAllItems().Where(i => i.supplier_id == supplierId);
        }

        public void AddItem(ItemsModel newItem)
        {
            var items = GetAllItems().ToList();
            newItem.created_at = DateTime.Now;
            newItem.updated_at = DateTime.Now;
            items.Add(newItem);
            SaveItems(items);
        }

        public void UpdateItem(ItemsModel updatedItem)
        {
            var items = GetAllItems().ToList();
            var existingItem = items.FirstOrDefault(i => i.uid == updatedItem.uid);

            if (existingItem != null)
            {
                existingItem.code = updatedItem.code;
                existingItem.description = updatedItem.description;
                existingItem.short_description = updatedItem.short_description;
                existingItem.upc_code = updatedItem.upc_code;
                existingItem.model_number = updatedItem.model_number;
                existingItem.commodity_code = updatedItem.commodity_code;
                existingItem.item_line = updatedItem.item_line;
                existingItem.item_group = updatedItem.item_group;
                existingItem.item_type = updatedItem.item_type;
                existingItem.unit_purchase_quantity = updatedItem.unit_purchase_quantity;
                existingItem.unit_order_quantity = updatedItem.unit_order_quantity;
                existingItem.pack_order_quantity = updatedItem.pack_order_quantity;
                existingItem.supplier_id = updatedItem.supplier_id;
                existingItem.supplier_code = updatedItem.supplier_code;
                existingItem.supplier_part_number = updatedItem.supplier_part_number;
                existingItem.updated_at = DateTime.Now;
                SaveItems(items);
            }
        }

        public void DeleteItem(string uid)
        {
            var items = GetAllItems().ToList();
            var itemToDelete = items.FirstOrDefault(i => i.uid == uid);

            if (itemToDelete != null)
            {
                items.Remove(itemToDelete);
                SaveItems(items);
            }
        }

        private void SaveItems(List<ItemsModel> items)
        {
            var jsonContent = JsonConvert.SerializeObject(items, Formatting.Indented);
            File.WriteAllText(dataFolder, jsonContent);
        }
    }

    public interface IItemsService
    {
        List<ItemsModel> GetAllItems();
        ItemsModel GetItemById(string uid);
        IEnumerable<ItemsModel> GetItemsByItemLine(int itemLineId);
        IEnumerable<ItemsModel> GetItemsByItemGroup(int itemGroupId);
        IEnumerable<ItemsModel> GetItemsByItemType(int itemTypeId);
        IEnumerable<ItemsModel> GetItemsBySupplierId(int supplierId);
        void AddItem(ItemsModel newItem);
        void UpdateItem(ItemsModel updatedItem);
        void DeleteItem(string uid);
    }
}
