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
            // Begin a database transaction
            using var transaction = await _Db.Database.BeginTransactionAsync();
            try
            {
                // Find the pending transfer
                var transfer = await _Db.Transfer
                                        .Include(t => t.items) // Ensure items are loaded
                                        .FirstOrDefaultAsync(x => x.id == id && x.transfer_status == "Pending");

                if (transfer == null)
                    throw new Exception("No pending transfer found with the given ID!");

                foreach (var item in transfer.items)
                {
                    // Get source inventory for the item
                    var sourceInventory = await _Db.Inventors
                                                .FirstOrDefaultAsync(inv => inv.item_id == item.item_id && inv.locations.Contains(transfer.transfer_from.Value));

                    if (sourceInventory == null)
                        throw new Exception($"Source inventory not found for item {item.item_id} at location {transfer.transfer_from}.");

                    if (sourceInventory.total_on_hand < item.amount)
                        throw new Exception($"Insufficient inventory for item {item.item_id} at location {transfer.transfer_from}.");

                    // Deduct items from the source location
                    sourceInventory.total_on_hand -= item.amount;
                    sourceInventory.total_expected = sourceInventory.total_on_hand + sourceInventory.total_ordered;
                    sourceInventory.total_available = sourceInventory.total_on_hand - sourceInventory.total_allocated;

                    _Db.Inventors.Update(sourceInventory);

                    // Get destination inventory for the item
                    var destinationInventory = await _Db.Inventors
                                                        .FirstOrDefaultAsync(inv => inv.item_id == item.item_id && inv.locations.Contains(transfer.transfer_to));

                    if (destinationInventory == null)
                        throw new Exception($"Destination inventory not found for item {item.item_id} at location {transfer.transfer_to}.");

                    // Add items to the destination location
                    destinationInventory.total_on_hand += item.amount;
                    destinationInventory.total_expected = destinationInventory.total_on_hand + destinationInventory.total_ordered;
                    destinationInventory.total_available = destinationInventory.total_on_hand - destinationInventory.total_allocated;

                    _Db.Inventors.Update(destinationInventory);
                }

                // Mark the transfer as completed
                transfer.transfer_status = "Completed";
                transfer.updated_at = DateTime.Now;

                _Db.Transfer.Update(transfer);

                // Save changes and commit the transaction
                await _Db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Rollback the transaction in case of any error
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task DeleteTransfer(int id)
        {
            var _transfer = await GetTransferById(id);
            _Db.Transfer.Remove(_transfer);
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
    }
}
