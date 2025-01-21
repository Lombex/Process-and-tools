using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface IItemsService
    {
        Task<List<ItemModel>> GetAllItems();
        Task<ItemModel> GetItemById(string uid);
        Task CreateItem(ItemModel item);
        Task UpdateItem(string uid, ItemModel item);
        Task DeleteItem(string uid);
        Task<IEnumerable<ItemModel>> GetItemsByLineId(int lineId);
        Task<IEnumerable<ItemModel>> GetItemsByGroupId(int groupId);
        Task<IEnumerable<ItemModel>> GetItemsBySupplierId(int supplierId);
        Task<IEnumerable<ItemModel>> GetItemsByTypeId(int item_type);
    }

    public class ItemsService : IItemsService
    {

        private readonly SQLiteDatabase _Db;
        private readonly HistoryService _historyService;
        public ItemsService(SQLiteDatabase sQLite, HistoryService historyService)
        {
            _Db = sQLite;
            _historyService = historyService;
        }
        public async Task<List<ItemModel>> GetAllItems() => await _Db.itemModels.AsQueryable().ToListAsync();
        public async Task<ItemModel> GetItemById(string uid)
        {
            var _item = await _Db.itemModels.FirstOrDefaultAsync(x => x.uid == uid);
            if (_item == null) throw new Exception("Item not found!");
            return _item;
        }
        public async Task CreateItem(ItemModel item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var amount = await GetAllItems();
            item.uid = $"P{(amount.Count + 1).ToString("D6")}";

            if (item.item_line > 0 && !(await _Db.ItemLine.AnyAsync(x => x.id == item.item_line))) throw new Exception("item_line does not exist!");
            if (item.item_group > 0 && !(await _Db.ItemGroups.AnyAsync(x => x.id == item.item_group))) throw new Exception("item_group does not exist!");
            if (item.item_type > 0 && !(await _Db.ItemType.AnyAsync(x => x.id == item.item_type))) throw new Exception("item_type does not exist!");

            await _Db.itemModels.AddAsync(item);
            await _Db.SaveChangesAsync();
             await _historyService.LogAsync(EntityType.Item, item.uid, "Created", $"Item {item.description} is aangemaakt met UID: {item.uid}");
        }
        public async Task UpdateItem(string uid, ItemModel item)
        {
            var _item = await GetItemById(uid);

            string changes = $"";
            if (_item.code != item.code) changes += $"Code: {_item.code} -> {item.code}; ";
            if (_item.description != item.description) changes += $"Description: {_item.description} -> {item.description}; ";
            if (_item.short_description != item.short_description) changes += $"Short Description: {_item.short_description} -> {item.short_description}; ";
            if (_item.upc_code != item.upc_code) changes += $"UPC Code: {_item.upc_code} -> {item.upc_code}; ";

            _item.code = item.code;
            _item.description = item.description;
            _item.short_description = item.short_description;
            _item.upc_code = item.upc_code;
            _item.model_number = item.model_number;
            _item.commodity_code = item.commodity_code;
            _item.item_line = item.item_line;
            _item.item_group = item.item_group;
            _item.item_type = item.item_type;
            _item.unit_purchase_quantity = item.unit_purchase_quantity;
            _item.unit_order_quantity = item.unit_order_quantity;
            _item.pack_order_quantity = item.pack_order_quantity;
            _item.supplier_id = item.supplier_id;
            _item.supplier_code = item.supplier_code;
            _item.supplier_part_number = item.supplier_part_number;
            _item.updated_at = DateTime.UtcNow;

            _Db.itemModels.Update(_item);
            await _Db.SaveChangesAsync();
            await _historyService.LogAsync(EntityType.Item, uid, "Updated", $"Wijzigingen: {changes}");

        }
        public async Task DeleteItem(string uid)
        {
            var item = await GetItemById(uid);
            if (item == null) throw new Exception("Item not found!");

            // Maak een kopie in de archieftabel
            var archivedItem = new ArchivedItemModel
            {
                uid = item.uid,
                description = item.description,
                short_description = item.short_description,
                upc_code = item.upc_code,
                model_number = item.model_number,
                commodity_code = item.commodity_code,
                item_line = item.item_line,
                item_group = item.item_group,
                item_type = item.item_type,
                unit_purchase_quantity = item.unit_purchase_quantity,
                unit_order_quantity = item.unit_order_quantity,
                pack_order_quantity = item.pack_order_quantity,
                supplier_id = item.supplier_id,
                supplier_code = item.supplier_code,
                supplier_part_number = item.supplier_part_number,
                created_at = item.created_at,
                updated_at = item.updated_at,
                archived_at = DateTime.UtcNow
            };

            await _Db.ArchivedItems.AddAsync(archivedItem);

            // Log de verwijdering in de History-tabel
            await _historyService.LogAsync(
                EntityType.Item,
                uid,
                "Archived",
                $"Item {item.uid} is gearchiveerd in plaats van verwijderd."
            );

            // Verwijder het originele item
            _Db.itemModels.Remove(item);
            await _Db.SaveChangesAsync();
        }

        public async Task<IEnumerable<ItemModel>> GetItemsByLineId(int lineId)
        {
            var _items = await GetAllItems();
            return _items.Where(x => x.item_line == lineId);
        }
        public async Task<IEnumerable<ItemModel>> GetItemsByGroupId(int groupId)
        {
            var _items = await GetAllItems();
            return _items.Where(x => x.item_group == groupId);
        }
        public async Task<IEnumerable<ItemModel>> GetItemsBySupplierId(int supplierId)
        {
            var _items = await GetAllItems();
            return _items.Where(x => x.supplier_id == supplierId);
        }

                public async Task<IEnumerable<ItemModel>> GetItemsByTypeId(int item_type)
        {
            var _items = await GetAllItems();
            return _items.Where(x => x.item_type == item_type);
        }
    }
}