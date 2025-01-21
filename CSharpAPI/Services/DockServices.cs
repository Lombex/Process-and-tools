using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CSharpAPI.Data;
using CSharpAPI.Models;
using Microsoft.Identity.Client;

namespace CSharpAPI.Service
{
    public interface IDockService
    {
        Task<List<DockModel>> GetAllDocks();
        Task<DockModel> GetDockById(int id);

        Task AddDock(DockModel dock);
        Task UpdateDock(int id, DockModel dock);
        Task DeleteDock(int id);

        Task<DockModel> GetDockByWarhouseId(int warehouseId);
    }

    public class DockService : IDockService
    {
        private readonly SQLiteDatabase _Db;
        public DockService(SQLiteDatabase sQLite)
        {
            _Db = sQLite;
        }
        public async Task<List<DockModel>> GetAllDocks() => await _Db.DockModels.AsQueryable().ToListAsync();   

        public  async Task<DockModel> GetDockByWarhouseId(int warehouseId)
        {
            var dock = await _Db.DockModels.FirstOrDefaultAsync(d => d.warehouse_id == warehouseId);
            if (dock == null) throw new Exception("Dock not found!");
            return dock;
        }

        public async Task<DockModel> GetDockById(int id)
        {
            var dock = await _Db.DockModels.FirstOrDefaultAsync(d => d.id == id);
            return dock;
        }

        public async Task AddDock(DockModel dock)
        {
            if (dock == null) throw new ArgumentNullException(nameof(dock));
            await _Db.DockModels.AddAsync(dock);
            await _Db.SaveChangesAsync();
        }

        public async Task UpdateDock(int id, DockModel dock)
        {
            var existingDock = await _Db.DockModels.FirstOrDefaultAsync(d => d.id == id);
            if (existingDock != null)
            {
                existingDock.code = dock.code;
                existingDock.name = dock.name;
                existingDock.created_at = dock.created_at;
                existingDock.updated_at = dock.updated_at;

                _Db.DockModels.Update(existingDock);
                await _Db.SaveChangesAsync();
            }
            else
            throw new Exception("Dock not found!");
        }

        public async Task DeleteDock(int id)
        {
            var dock = await _Db.DockModels.FirstOrDefaultAsync(d => d.id == id);
            if (dock == null)
            {
                throw new Exception("Dock not found!");
            }

            // Maak een kopie in de archieftabel
            var archivedDock = new ArchivedDockModel
            {
                id = dock.id,
                warehouse_id = dock.warehouse_id,
                code = dock.code,
                name = dock.name,
                created_at = dock.created_at,
                updated_at = dock.updated_at,
                archived_at = DateTime.UtcNow // Tijdstip van archivering
            };

            await _Db.ArchivedDocks.AddAsync(archivedDock);

            // Verwijder het originele record
            _Db.DockModels.Remove(dock);

            // Sla wijzigingen op in de database
            await _Db.SaveChangesAsync();
        }
    }
}