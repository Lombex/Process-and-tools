using CSharpAPI.Data;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CSharpAPI.Service
{
    public interface IShipmentService
    {
        Task<List<ShipmentModel>> GetAll();
        Task<ShipmentModel> GetById(int id);
        Task Add(ShipmentModel shipment);
        Task Update(int id, ShipmentModel shipment);
        Task Delete(int id);
    }

    public class ShipmentService : IShipmentService
    {
        private readonly SQLiteDatabase _Db;

        public ShipmentService(SQLiteDatabase sQLite)
        {
            _Db = sQLite;
        }

        public async Task<List<ShipmentModel>> GetAll() => await _Db.Shipment.AsQueryable().ToListAsync();

        public async Task<ShipmentModel> GetById(int id)
        {
            var _shipment = await _Db.Shipment.FirstOrDefaultAsync(x => x.id == id);
            if (_shipment == null) throw new Exception("Shipment not found!");
            return _shipment;
        }

        public async Task Add(ShipmentModel shipment)
        {
            if (shipment == null) throw new ArgumentNullException(nameof(shipment));
            await _Db.Shipment.AddAsync(shipment);
            await _Db.SaveChangesAsync();
        }

        public async Task Update(int id, ShipmentModel shipment)
        {
            var _shipment = await GetById(id);

            _shipment.order_id = shipment.order_id;
            _shipment.source_id = shipment.source_id;
            _shipment.order_date = shipment.order_date;
            _shipment.request_date = shipment.request_date;
            _shipment.shipment_date = shipment.shipment_date;
            _shipment.shipment_type = shipment.shipment_type;
            _shipment.shipment_status = shipment.shipment_status;
            _shipment.notes = shipment.notes;
            _shipment.carrier_code = shipment.carrier_code;
            _shipment.carrier_description = shipment.carrier_description;
            _shipment.service_code = shipment.service_code;
            _shipment.payment_type = shipment.payment_type;
            _shipment.transfer_mode = shipment.transfer_mode;
            _shipment.total_package_count = shipment.total_package_count;
            _shipment.total_package_weight = shipment.total_package_weight;
            _shipment.items = shipment.items;
            _shipment.updated_at = DateTime.Now;

            _Db.Shipment.Update(shipment);
            await _Db.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var _shipment = await GetById(id);
            _Db.Shipment.Remove(_shipment);
            await _Db.SaveChangesAsync();
        }
    }
}