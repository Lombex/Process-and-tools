using Newtonsoft.Json;
using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Service
{
    public class WarehouseService : IWarehouseService
    {
        private readonly SQLiteDatabase _Db;

        public WarehouseService(SQLiteDatabase sQLite)
        {
            _Db = sQLite;
        }

        public async Task<List<WarehouseModel>> GetAllWarehouses() => await _Db.Warehouse.AsQueryable().ToListAsync();  
        public async Task<WarehouseModel> GetWarehouseById(int id)
        {
            var _warehouse = await _Db.Warehouse.FirstOrDefaultAsync(x => x.id == id);
            if (_warehouse == null) throw new Exception("Warehouse not found!");
            return _warehouse;
        } 
        public async Task<List<LocationModel>> GetLocationFromWarehouseID(int id)
        {
            var _warehouse = await GetWarehouseById(id);
            var _locations = await _Db.Location.Where(x => x.warehouse_id == _warehouse.id).ToListAsync();
            return _locations;
        }
        public async Task AddWarehouse(WarehouseModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await _Db.Warehouse.AddAsync(model);
            await _Db.SaveChangesAsync();
        }
        public async Task UpdateWarehouse(int id, WarehouseModel model)
        {
            var _warehouse = await GetWarehouseById(id);

            _warehouse.code = model.code;
            _warehouse.name = model.name;
            _warehouse.address = model.address;
            _warehouse.zip = model.zip;
            _warehouse.city = model.city;
            _warehouse.province = model.province;
            _warehouse.country = model.country;
            _warehouse.contact = model.contact;
            _warehouse.updated_at = DateTime.Now;

            _Db.Warehouse.Update(_warehouse);
            await _Db.SaveChangesAsync();
        }
        public async Task DeleteWarehouse(int id)
        {
            var _warehouse = await GetWarehouseById(id);
            _Db.Warehouse.Remove(_warehouse);
            await _Db.SaveChangesAsync();
        }
    }
    public interface IWarehouseService
    {
        Task<List<WarehouseModel>> GetAllWarehouses();
        Task<WarehouseModel> GetWarehouseById(int id);
        Task<List<LocationModel>> GetLocationFromWarehouseID(int id);
        Task AddWarehouse(WarehouseModel model);
        Task UpdateWarehouse(int id, WarehouseModel model);
        Task DeleteWarehouse(int id);
    }
}
