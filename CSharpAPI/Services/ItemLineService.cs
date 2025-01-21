using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Service
{
    public interface IItemLineService
    {
        Task<List<ItemLineModel>> GetAllItemLines();
        Task<ItemLineModel> GetItemLineById(int id);
        Task CreateItemLine(ItemLineModel itemLine);
        Task<bool> UpdateItemLine(int id, ItemLineModel itemLine);
        Task<bool> DeleteItemLine(int id);
    }

    public class ItemLineService : IItemLineService
    {
        private readonly SQLiteDatabase _Db;


        public ItemLineService(SQLiteDatabase context)
        {
            _Db = context;
        }

        public async Task<List<ItemLineModel>> GetAllItemLines()
        {
            return await _Db.ItemLine.ToListAsync();
        }

        public async Task<ItemLineModel> GetItemLineById(int id)
        {
            var itemLine = await _Db.ItemLine.FirstOrDefaultAsync(x => x.id == id);
            if (itemLine == null)
            {
                throw new Exception($"ItemLine with id {id} not found.");
            }
            return itemLine;
        }

        public async Task CreateItemLine(ItemLineModel itemLine)
        {
            itemLine.created_at = DateTime.UtcNow;
            itemLine.updated_at = DateTime.UtcNow;
            await _Db.ItemLine.AddAsync(itemLine);
            await _Db.SaveChangesAsync();
        }

        public async Task<bool> UpdateItemLine(int id, ItemLineModel itemLine)
        {
            var existingItemLine = await _Db.ItemLine.FirstOrDefaultAsync(x => x.id == id);
            if (existingItemLine == null) return false;

            existingItemLine.name = itemLine.name;
            existingItemLine.description = itemLine.description;
            existingItemLine.updated_at = DateTime.UtcNow;

            _Db.ItemLine.Update(existingItemLine);
            await _Db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteItemLine(int id)
        {
            var itemLine = await _Db.ItemLine.FirstOrDefaultAsync(x => x.id == id);
            if (itemLine == null) return false;

            // Maak een kopie in de archieftabel
            var archivedItemLine = new ArchivedItemLineModel
            {
                id = itemLine.id,
                name = itemLine.name,
                description = itemLine.description,
                created_at = itemLine.created_at,
                updated_at = itemLine.updated_at,
                archived_at = DateTime.UtcNow // Tijdstip van archivering
            };

            await _Db.ArchivedItemLines.AddAsync(archivedItemLine);

            // Verwijder het originele record
            _Db.ItemLine.Remove(itemLine);

            // Sla wijzigingen op in de database
            await _Db.SaveChangesAsync();

            return true;
        }
    }
}