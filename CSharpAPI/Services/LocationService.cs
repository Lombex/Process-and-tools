using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Service
{
    public interface ILocationService
    {
        Task<List<LocationModel>> GetAll();
        Task<LocationModel> GetById(int id);
        Task Add(LocationModel location);
        Task<bool> Update(int id, LocationModel location);
        Task<bool> Delete(int id);
        Task<List<LocationModel>> GetByWarehouseId(int warehouseId);
    }

    public class LocationService : ILocationService
    {
        private readonly SQLiteDatabase _Db;

        public LocationService(SQLiteDatabase db)
        {
            _Db = db;
        }

        public async Task<List<LocationModel>> GetAll()
        {
            return await _Db.Location.ToListAsync();
        }

        public async Task<LocationModel> GetById(int id)
        {
            var location = await _Db.Location.FirstOrDefaultAsync(x => x.id == id);
            if (location == null)
            {
                throw new Exception($"Location {id} not found");
            }
            return location;
        }

        public async Task Add(LocationModel location)
        {
            location.created_at = DateTime.UtcNow;
            location.updated_at = DateTime.UtcNow;
            await _Db.Location.AddAsync(location);
            await _Db.SaveChangesAsync();
        }

        public async Task<bool> Update(int id, LocationModel location)
        {
            var existingLocation = await _Db.Location.FirstOrDefaultAsync(x => x.id == id);
            if (existingLocation == null)
            {
                return false;
            }

            existingLocation.warehouse_id = location.warehouse_id;
            existingLocation.code = location.code;
            existingLocation.name = location.name;
            existingLocation.updated_at = DateTime.UtcNow;

            _Db.Location.Update(existingLocation);
            await _Db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var location = await _Db.Location.FirstOrDefaultAsync(x => x.id == id);
            if (location == null)
            {
                return false;
            }

            // Maak een kopie in de archieftabel
            var archivedLocation = new ArchivedLocationModel
            {
                id = location.id,
                warehouse_id = location.warehouse_id,
                code = location.code,
                name = location.name,
                created_at = location.created_at,
                updated_at = location.updated_at,
                archived_at = DateTime.UtcNow // Tijdstip van archivering
            };

            await _Db.ArchivedLocations.AddAsync(archivedLocation);

            // Verwijder het originele record
            _Db.Location.Remove(location);

            // Sla wijzigingen op in de database
            await _Db.SaveChangesAsync();

            return true;
        }

        public async Task<List<LocationModel>> GetByWarehouseId(int warehouseId)
        {
            return await _Db.Location.Where(x => x.warehouse_id == warehouseId).ToListAsync();
        }
    }
}