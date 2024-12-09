using CSharpAPI.Data;
using CSharpAPI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface ISupplierService
    {
        Task<List<SupplierModel>> GetAllSuppliers();
        Task<SupplierModel> GetSupplierById(int id);
        Task<List<ItemModel>> GetItemFromSupplierId(int id);
        Task UpdateSupplier(int id, SupplierModel supplier);
        Task CreateSupplier(SupplierModel supplier);
        Task DeleteSupplier(int id);
    }
    public class SupplierService : ISupplierService
    {
        private readonly SQLiteDatabase _Db;

        public SupplierService(SQLiteDatabase sQLite)
        {
            _Db = sQLite;
        }

        public async Task<List<SupplierModel>> GetAllSuppliers() => await _Db.Suppliers.AsQueryable().ToListAsync();

        public async Task<SupplierModel> GetSupplierById(int id)
        {
            var _supplier = await _Db.Suppliers.FirstOrDefaultAsync(x => x.id == id);
            if (_supplier == null) throw new Exception("Supplier not found!");
            return _supplier;
        }

        // Has to be implemented
        public async Task<List<ItemModel>> GetItemFromSupplierId(int id)
        {
            var _supplierid = await GetSupplierById(id);
            var _items = await _Db.itemModels.Where(x => x.supplier_id == _supplierid.id).ToListAsync();
            return _items;
        }

        public async Task UpdateSupplier(int id, SupplierModel updateSupplier)
        {
            var _supplier = await GetSupplierById(id);

            _supplier.code = updateSupplier.code;
            _supplier.name = updateSupplier.name;
            _supplier.address = updateSupplier.address;
            _supplier.address_extra = updateSupplier.address_extra;
            _supplier.city = updateSupplier.city;
            _supplier.zip_code = updateSupplier.zip_code;
            _supplier.province = updateSupplier.province;
            _supplier.contact_name = updateSupplier.contact_name;
            _supplier.phonenumber = updateSupplier.phonenumber;
            _supplier.reference = updateSupplier.reference;
            _supplier.updated_at = DateTime.Now;

            _Db.Suppliers.Update(_supplier);
            await _Db.SaveChangesAsync();
        }

        public async Task CreateSupplier(SupplierModel supplier)
        {
            if (supplier == null) throw new ArgumentNullException(nameof(supplier));
            await _Db.Suppliers.AddAsync(supplier);
            await _Db.SaveChangesAsync();
        }

        public async Task DeleteSupplier(int id)
        {
            var _supplier = await GetSupplierById(id);
            _Db.Suppliers.Remove(_supplier);
            await _Db.SaveChangesAsync();
        }
    }
} 