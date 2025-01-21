using CSharpAPI.Data;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface IItemTypeService
    {
        Task<List<ItemTypeModel>> GetAll();
        Task<ItemTypeModel> GetById(int id);
        Task Add(ItemTypeModel itemType);
        Task Update(int id, ItemTypeModel itemType);
        Task Delete(int id);
    }

    public class ItemTypeService : IItemTypeService
    {
        private readonly SQLiteDatabase _Db;
        public ItemTypeService(SQLiteDatabase sQLite)
        {
            _Db = sQLite; 
        }
        public async Task<List<ItemTypeModel>> GetAll() => await _Db.ItemType.AsQueryable().ToListAsync();
        public async Task<ItemTypeModel> GetById(int id)
        {
            var _itemtype = await _Db.ItemType.FirstOrDefaultAsync(x => x.id == id);
            if (_itemtype == null) throw new Exception($"ItemType not found!");
            return _itemtype;
        }
        public async Task Add(ItemTypeModel itemType)
        {
            if (itemType == null) throw new ArgumentNullException(nameof(itemType));
            await _Db.ItemType.AddAsync(itemType);
            await _Db.SaveChangesAsync();
        }
        public async Task Update(int id, ItemTypeModel itemType)
        {
            var _itemtype = await GetById(id);

            _itemtype.name = itemType.name;
            _itemtype.description = itemType.description;
            _itemtype.updated_at = DateTime.Now;

            _Db.ItemType.Update(itemType);
            await _Db.SaveChangesAsync();
        }
        public async Task Delete(int id)
        {
            var _itemtype = await GetById(id);
            if (_itemtype == null) throw new Exception("ItemType not found!");

            // Maak een kopie in de archieftabel
            var archivedItemType = new ArchivedItemTypeModel
            {
                id = _itemtype.id,
                name = _itemtype.name,
                description = _itemtype.description,
                created_at = _itemtype.created_at,
                updated_at = _itemtype.updated_at,
                archived_at = DateTime.UtcNow // Tijdstip van archivering
            };

            await _Db.ArchivedItemTypes.AddAsync(archivedItemType);

            // Verwijder het originele record
            _Db.ItemType.Remove(_itemtype);

            // Sla wijzigingen op in de database
            await _Db.SaveChangesAsync();
        }
    }
}