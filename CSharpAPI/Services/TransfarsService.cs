using CSharpAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;

namespace CSharpAPI.Service {
    public class TransferSerivce : ITransfersService {
        private readonly string dummydata = "data/transfer.json";

        public List<TransferModel> GetAllTransfers() {
            if (!File.Exists(dummydata)) return new List<TransferModel>();
            return JsonConvert.DeserializeObject<List<TransferModel>>(File.ReadAllText(dummydata)) ?? new List<TransferModel>();

            // still returns our dummy data
        }

        public TransferModel GetTransferById(int id) {
            var _transfer = GetAllTransfers().FirstOrDefault(x => x.id == id);
            if (_transfer == null) throw new Exception("This Transfer does not exits!");
            return _transfer;
        }

        public List<Items> GetItemFromTransferId(int id) {
            var _transfer = GetAllTransfers().FirstOrDefault(x => x.id == id);
            if (_transfer == null) throw new Exception("This Transfer does not exits!");
            if (_transfer.items == null) throw new Exception("This Transfer does not contain any items!");
            return _transfer.items;
        }

        public bool UpdateTransfer(int id, TransferModel updateTransfer) {
            var _transfer = GetAllTransfers().FirstOrDefault(x => x.id == id);
            if (_transfer == null) return false;

            updateTransfer.id = _transfer.id;
            updateTransfer.reference = _transfer.reference;
            updateTransfer.transfer_from = _transfer.transfer_from;
            updateTransfer.transfer_to = _transfer.transfer_to;
            updateTransfer.transfer_status = _transfer.transfer_status;
            updateTransfer.created_at = _transfer.created_at;
            updateTransfer.updated_at = _transfer.updated_at;
            updateTransfer.items = _transfer.items;

            // Update Database here.

            return true;
        }

        public void CreateTransfer(TransferModel transfer) {
            var AllTransfers = GetAllTransfers();
            transfer.id = AllTransfers.Count > 0 ? AllTransfers.Max(x => x.id) + 1 : 1;
            AllTransfers.Add(transfer);

            // Update Database here.
        }

        public bool DeleteTransfer(int id) {      
            var _transfer = GetAllTransfers().FirstOrDefault(x => x.id == id);
            if (_transfer == null) return false;
            
            // Update Database here.

            return true;
        }

    }

    public interface ITransfersService {
        List<TransferModel> GetAllTransfers();
        TransferModel GetTransferById(int id);
        List<Items> GetItemFromTransferId(int id);
        bool UpdateTransfer(int id, TransferModel updateTransfer);

        // commit => CommitTransfer();
        void CreateTransfer(TransferModel transfer);
        bool DeleteTransfer(int id);
    }
}