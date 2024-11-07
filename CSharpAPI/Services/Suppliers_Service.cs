using CSharpAPI.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface ISupplierService
    {
        List<SuppliersModel> GetAllSuppliers();
        SuppliersModel GetSupplierById(int id);
        List<Items> GetItemFromSupplierId(int id);
        bool UpdateSupplier(int id, SuppliersModel supplier);
        void CreateSupplier(SuppliersModel supplier);
        bool DeleteSupplier(int id);
    }
    public class SupplierService : ISupplierService
    {
        private readonly string dummydata = "data/transfer.json";

        public List<SuppliersModel> GetAllSuppliers()
        {
            if (!File.Exists(dummydata)) return new List<SuppliersModel>();
            return JsonConvert.DeserializeObject<List<SuppliersModel>>(File.ReadAllText(dummydata)) ?? new List<SuppliersModel>();
        }

        public SuppliersModel GetSupplierById(int id)
        {
            var _supplier = GetAllSuppliers().FirstOrDefault(x => x.id == id);
            if (_supplier == null) throw new Exception("This Supplier does not exits!");
            return _supplier;
        }

        public List<Items> GetItemFromSupplierId(int id)
        {
            var _supplier = GetAllSuppliers().FirstOrDefault(x => x.id == id);
            if (_supplier == null) throw new Exception("This Supplier does not exits!");
            // Get list of items and get value of "supplier_id"
            // if "supplier_id" is same as _supplier.id return it.
            throw new NotImplementedException();
        }

        public bool UpdateSupplier(int id, SuppliersModel updateSupplier)
        {
            var _supplier = GetAllSuppliers().FirstOrDefault(_x => _x.id == id);
            if (_supplier == null) return false;

            updateSupplier.id = _supplier.id;
            updateSupplier.code = _supplier.code;
            updateSupplier.name = _supplier.name;
            updateSupplier.address = _supplier.address;
            updateSupplier.address_extra = _supplier.address_extra;
            updateSupplier.city = _supplier.city;
            updateSupplier.zip_code = _supplier.zip_code;
            updateSupplier.province = _supplier.province;
            updateSupplier.contact_name = _supplier.contact_name;
            updateSupplier.phonenumber = _supplier.phonenumber;
            updateSupplier.reference = _supplier.reference;

            updateSupplier.updated_at = DateTime.Now;

            return true;

        }

        public void CreateSupplier(SuppliersModel supplier)
        {
            var AllSuppliers = GetAllSuppliers();
            supplier.id = AllSuppliers.Count > 0 ? AllSuppliers.Max(x => x.id) + 1 : 1;
            AllSuppliers.Add(supplier);
        }

        public bool DeleteSupplier(int id)
        {
            var _supplier = GetAllSuppliers().FirstOrDefault(x => x.id == id);
            if (_supplier == null) return false;
            // Change Database

            return true;
        }
    }
} 