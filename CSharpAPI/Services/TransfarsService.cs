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

        // This has to be confirmed...
        public async Task CommitTransfer(int id)
        {
            var _transfer = await GetTransferById(id);
            foreach (Items? _item in _transfer.items)
            {
                var _inventory = _Db.Inventors.Where(x => x.item_id == _item.item_id);
                foreach (var y in _inventory)
                {
                    if (y.locations.Any(x => x == _transfer.transfer_from)) 
                    {
                        y.total_on_hand -= _item.amount;
                        y.total_expected = y.total_on_hand + y.total_ordered;
                        y.total_available = y.total_on_hand - y.total_allocated;
                        _Db.Inventors.Update(y);
                        await _Db.SaveChangesAsync();
                    } else if (y.locations.Any(x => x == _transfer.transfer_to))
                    {
                        y.total_on_hand += _item.amount;
                        y.total_expected = y.total_on_hand + y.total_ordered;
                        y.total_available = y.total_on_hand - y.total_allocated;
                        _Db.Inventors.Update(y);
                        await _Db.SaveChangesAsync();
                    }
                }
            } 
            _transfer.transfer_status = "Processed";
            await UpdateTransfer(id, _transfer);
            await _Db.SaveChangesAsync();
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

        // commit => CommitTransfer(); 
        Task DeleteTransfer(int id);
    }
}