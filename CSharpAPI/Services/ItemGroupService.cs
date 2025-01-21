using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Service
{
    public interface IItemGroupService
    {
        Task<List<ItemGroupModel>> GetAll();
        Task<ItemGroupModel> GetById(int id);
        Task Add(ItemGroupModel itemGroupModel);
        Task<bool> Update(int id, ItemGroupModel itemGroupModel);
        Task<bool> Delete(int id);
    }

    public class ItemGroupService : IItemGroupService
    {
        private readonly SQLiteDatabase _Db;

        public ItemGroupService(SQLiteDatabase context)
        {
            _Db = context;
        }

        public async Task<List<ItemGroupModel>> GetAll()
        {
            return await _Db.ItemGroups.ToListAsync();
        }

        public async Task<ItemGroupModel> GetById(int id)
        {
            var itemGroup = await _Db.ItemGroups.FirstOrDefaultAsync(x => x.id == id);
            if (itemGroup == null)
            {
                throw new Exception($"ItemGroup {id} not found");
            }
            return itemGroup;
        }

        public async Task Add(ItemGroupModel itemGroupModel)
        {
            itemGroupModel.created_at = DateTime.UtcNow;
            itemGroupModel.updated_at = DateTime.UtcNow;
            await _Db.ItemGroups.AddAsync(itemGroupModel);
            await _Db.SaveChangesAsync();
        }

        public async Task<bool> Update(int id, ItemGroupModel itemGroupModel)
        {
            var existingItemGroup = await _Db.ItemGroups.FirstOrDefaultAsync(x => x.id == id);
            if (existingItemGroup == null) return false;

            existingItemGroup.name = itemGroupModel.name;
            existingItemGroup.description = itemGroupModel.description;
            existingItemGroup.itemtype_id = itemGroupModel.itemtype_id;
            existingItemGroup.updated_at = DateTime.UtcNow;

            _Db.ItemGroups.Update(existingItemGroup);
            await _Db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var itemGroup = await _Db.ItemGroups.FirstOrDefaultAsync(x => x.id == id);
            if (itemGroup == null) return false;

            // Maak een kopie in de archieftabel
            var archivedItemGroup = new ArchivedItemGroupModel
            {
                id = itemGroup.id,
                name = itemGroup.name,
                description = itemGroup.description,
                itemtype_id = itemGroup.itemtype_id,
                created_at = itemGroup.created_at,
                updated_at = itemGroup.updated_at,
                archived_at = DateTime.UtcNow // Tijdstip van archivering
            };

            await _Db.ArchivedItemGroups.AddAsync(archivedItemGroup);

            // Verwijder het originele record
            _Db.ItemGroups.Remove(itemGroup);

            // Sla wijzigingen op in de database
            await _Db.SaveChangesAsync();

            return true;
        }

    }
}