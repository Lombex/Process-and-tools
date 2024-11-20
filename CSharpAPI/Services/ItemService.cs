using CSharpAPI.Models;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface IItemsService
    {
        IEnumerable<ItemModel> GetAllItems();
        ItemModel GetItemById(string uid);
        void CreateItem(ItemModel item);
        bool UpdateItem(string uid, ItemModel item);
        bool DeleteItem(string uid);
        IEnumerable<ItemModel> GetItemsByLineId(int lineId);
        IEnumerable<ItemModel> GetItemsByGroupId(int groupId);
        IEnumerable<ItemModel> GetItemsBySupplierId(int supplierId);
    }

    public class ItemsService : IItemsService
    {
        private List<ItemModel> _items;

        public ItemsService()
        {
            _items = new List<ItemModel>
            {
                new ItemModel
                {
                    uid = "ITEM001",
                    code = "DEFAULT001",
                    description = "Default Item",
                    item_line = 0,
                    item_group = 0,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };
        }

        public IEnumerable<ItemModel> GetAllItems() => _items;

        public ItemModel GetItemById(string uid)
        {
            var item = _items.FirstOrDefault(x => x.uid == uid);
            if (item == null)
            {
                throw new Exception($"Item with uid {uid} not found");
            }
            return item;
        }

        public void CreateItem(ItemModel item)
        {
            item.uid = GenerateUniqueId(); 
            item.created_at = DateTime.UtcNow;
            item.updated_at = DateTime.UtcNow;
            _items.Add(item);
        }

        public bool UpdateItem(string uid, ItemModel item)
        {
            var existingItem = _items.FirstOrDefault(x => x.uid == uid);
            if (existingItem == null) return false;

            existingItem.code = item.code;
            existingItem.description = item.description;
            existingItem.short_description = item.short_description;
            existingItem.upc_code = item.upc_code;
            existingItem.model_number = item.model_number;
            existingItem.commodity_code = item.commodity_code;
            existingItem.item_line = item.item_line;
            existingItem.item_group = item.item_group;
            existingItem.item_type = item.item_type;
            existingItem.unit_purchase_quantity = item.unit_purchase_quantity;
            existingItem.unit_order_quantity = item.unit_order_quantity;
            existingItem.pack_order_quantity = item.pack_order_quantity;
            existingItem.supplier_id = item.supplier_id;
            existingItem.supplier_code = item.supplier_code;
            existingItem.supplier_part_number = item.supplier_part_number;
            existingItem.updated_at = DateTime.UtcNow;

            return true;
        }

        public bool DeleteItem(string uid)
        {
            var item = _items.FirstOrDefault(x => x.uid == uid);
            if (item == null) return false;

            _items.Remove(item);
            return true;
        }

        public IEnumerable<ItemModel> GetItemsByLineId(int lineId)
        {
            return _items.Where(i => i.item_line == lineId);
        }

        public IEnumerable<ItemModel> GetItemsByGroupId(int groupId)
        {
            return _items.Where(i => i.item_group == groupId);
        }

        public IEnumerable<ItemModel> GetItemsBySupplierId(int supplierId)
        {
            return _items.Where(i => i.supplier_id == supplierId);
        }

        private string GenerateUniqueId()
        {
            return "ITEM" + DateTime.UtcNow.Ticks.ToString();
        }
    }
}