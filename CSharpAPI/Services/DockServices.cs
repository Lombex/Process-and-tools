using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CSharpAPI.Data;
using CSharpAPI.Models;

namespace CSharpAPI.Service
{
    public interface IDockService
    {
        Task<List<DockModel>> GetAllDocks();
        Task<DockModel> GetDockById(int id);
        Task AddDock(DockModel dock);
        Task UpdateDock(int id, DockModel dock);
        Task DeleteDock(int id);
        Task<List<DockModel>> GetDocksByWarehouseId(int warehouseId);
    }

    public class DockService : IDockService
    {
        private readonly SQLiteDatabase _Db;
        public DockService(SQLiteDatabase sQLite)
        {
            _Db = sQLite;
        }

        public async Task<List<DockModel>> GetAllDocks() => await _Db.DockModels.AsQueryable().ToListAsync();   

        public async Task<List<DockModel>> GetDocksByWarehouseId(int warehouseId)
        {
            var docks = await _Db.DockModels.Where(d => d.warehouse_id == warehouseId).ToListAsync();
            if (docks == null || !docks.Any()) throw new Exception("No docks found for this warehouse!");
            return docks;
        }

        public async Task<DockModel> GetDockById(int id)
        {
            var dock = await _Db.DockModels.FirstOrDefaultAsync(d => d.id == id);
            if (dock == null) throw new Exception("Dock not found!");
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

            // Create a copy in the archive table
            var archivedDock = new ArchivedDockModel
            {
                id = dock.id,
                warehouse_id = dock.warehouse_id,
                code = dock.code,
                name = dock.name,
                created_at = dock.created_at,
                updated_at = dock.updated_at,
                archived_at = DateTime.UtcNow // Time of archiving
            };

            await _Db.ArchivedDocks.AddAsync(archivedDock);

            // Remove the original record
            _Db.DockModels.Remove(dock);

            // Save changes to the database
            await _Db.SaveChangesAsync();
        }
    }
}