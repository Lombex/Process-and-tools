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
            throw new NotImplementedException();
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

        // Has to be implemented 
        public async Task CommitTransfer()
        {
            throw new NotImplementedException();
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