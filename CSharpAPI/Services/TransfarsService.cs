using CSharpAPI.Data;
using CSharpAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CSharpAPI.Service {
    public class TransferSerivce : ITransfersService {

        private readonly SQLiteDatabase _Db;
        public TransferSerivce(SQLiteDatabase sQLite) 
        {
            _Db = sQLite;
        }

        public async Task<List<TransferModel>> GetAllTransfers() => await _Db.Transfer.AsQueryable().ToListAsync();

        public async Task<TransferModel> GetTransferById(int id)
        {
            var _transfer = await _Db.Transfer.FirstOrDefaultAsync(x => x.id == id);
            if (_transfer == null) throw new Exception("Transfer not found!");
            return _transfer;
        }

        // Has to be implemented
        public async Task<List<Items>> GetItemFromTransferId(int id)
        {
            var _transfer = await GetTransferById(id);
            return _transfer.items;
        }

        public async Task CreateTransfer(TransferModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await _Db.Transfer.AddAsync(model);
            await _Db.SaveChangesAsync();
        }

        public async Task UpdateTransfer(int id, TransferModel model)
        {
            var _transfer = await GetTransferById(id);

            _transfer.reference = model.reference;
            _transfer.transfer_from = model.transfer_from;
            _transfer.transfer_to = model.transfer_to;
            _transfer.transfer_status = model.transfer_status;
            _transfer.updated_at = DateTime.Now;
            _transfer.items = model.items;

            _Db.Transfer.Update(_transfer);
            await _Db.SaveChangesAsync();
        }

        // Commit a transfer
        public async Task CommitTransfer(int id)
        {
            var transfer = await _Db.Transfer
                                    .Where(x => x.id == id && x.transfer_status == "Pending")
                                    .FirstOrDefaultAsync();
            if (transfer == null) throw new Exception("No pending transfer found with the given ID!");
            foreach (var item in transfer.items)
            {
                var inventories = await _Db.Inventors
                                            .Where(y => y.item_id == item.item_id)
                                            .ToListAsync();

                foreach (var inventory in inventories)
                {
                    if (inventory.locations.Contains((int)transfer.transfer_from))
                    {
                        // verminder de aantalen op de locatie
                        inventory.total_on_hand -= item.amount;
                        inventory.total_expected = inventory.total_on_hand + inventory.total_ordered;
                        inventory.total_available = inventory.total_on_hand - inventory.total_allocated;
                        _Db.Inventors.Update(inventory);
                    }
                    else if (inventory.locations.Contains((int)transfer.transfer_to))
                    {
                        //optellen van de aantallen op de locatie
                        inventory.total_on_hand += item.amount;
                        inventory.total_expected = inventory.total_on_hand + inventory.total_ordered;
                        inventory.total_available = inventory.total_on_hand - inventory.total_allocated;
                        _Db.Inventors.Update(inventory);
                    }
                }
            }
            transfer.transfer_status = "Completed";
            transfer.updated_at = DateTime.Now;

            _Db.Transfer.Update(transfer);
            await _Db.SaveChangesAsync();
        }


        public async Task DeleteTransfer(int id)
        {
            var _transfer = await GetTransferById(id);
            _Db.Transfer.Remove(_transfer);
            await _Db.SaveChangesAsync();
        }
        public async Task TransferToLocation(int id, int locationId)
        {
            var transfer = await GetTransferById(id);
            var location = await _Db.Location.FirstOrDefaultAsync(x => x.id == locationId);
            if (location == null) throw new Exception("Location not found!");
            transfer.transfer_to = locationId;
            transfer.updated_at = DateTime.Now;
            _Db.Transfer.Update(transfer);
            await _Db.SaveChangesAsync();
        }

        public async Task TransferFromLocation(int id, int locationId)
        {
            var transfer = await GetTransferById(id);

            // Handle dock-specific logic if transfer_from is null
            if (transfer.transfer_from == null)
            {
                var dock = await _Db.DockModels.FirstOrDefaultAsync(d => d.id == locationId);
                if (dock == null) throw new Exception("Dock not found!");
            }
            transfer.transfer_from = locationId;
            transfer.updated_at = DateTime.Now;
            _Db.Transfer.Update(transfer);
            await _Db.SaveChangesAsync();
        }


        
    }

    public interface ITransfersService {
        Task<List<TransferModel>> GetAllTransfers();
        Task<TransferModel> GetTransferById(int id);
        Task<List<Items>> GetItemFromTransferId(int id);
        Task CreateTransfer(TransferModel transfer);
        Task UpdateTransfer(int id, TransferModel updateTransfer);
        Task CommitTransfer(int id);
        Task DeleteTransfer(int id);
        Task TransferToLocation(int id, int location);
        Task TransferFromLocation(int id, int location);
    }
}
