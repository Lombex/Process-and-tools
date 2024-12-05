using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services
{
    public interface IClientsService
    {
        Task<List<ClientModel>> GetAllClients();
        Task<ClientModel> GetClientById(int id);
        Task<List<OrderModel>> GetClientOrders(int id);
        Task AddClient(ClientModel client);
        Task UpdateClient(int id, ClientModel client);
        Task DeleteClient(int id);
    }

    public class ClientsService : IClientsService
    {
        private readonly SQLiteDatabase _Db;

        public ClientsService(SQLiteDatabase sQLite)
        {
            _Db = sQLite;
        }

        public async Task<List<ClientModel>> GetAllClients()
        {
            return await _Db.ClientModels.AsQueryable().ToListAsync();
        }

        public async Task<ClientModel> GetClientById(int id)
        {
            var client = await _Db.ClientModels.FirstOrDefaultAsync(c => c.id == id);
            if (client == null) throw new Exception("Client not found!");
            return client;
        }

        public async Task<List<OrderModel>> GetClientOrders(int id)
        {
            var _client = await GetClientById(id);
            var _order = await _Db.Order.Where(x => x.bill_to == _client.id || x.ship_to == _client.id).ToListAsync();
            return _order;
        }

        public async Task AddClient(ClientModel client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            client.created_at = DateTime.Now;
            client.updated_at = DateTime.Now;
            await _Db.ClientModels.AddAsync(client);
            await _Db.SaveChangesAsync();
        }

        public async Task UpdateClient(int id, ClientModel client)
        {
            var existingClient = await GetClientById(id);
            if (existingClient == null) throw new Exception("Client not found!");

            existingClient.name = client.name;
            existingClient.address = client.address;
            existingClient.contact_email = client.contact_email;
            existingClient.contact_phone = client.contact_phone;
            existingClient.city = client.city;
            existingClient.zip_code = client.zip_code;
            existingClient.province = client.province;
            existingClient.country = client.country;
            existingClient.contact_name = client.contact_name;
            existingClient.updated_at = DateTime.Now;

            _Db.ClientModels.Update(existingClient);
            await _Db.SaveChangesAsync();
        }

        public async Task DeleteClient(int id)
        {
            var client = await GetClientById(id);
            if (client == null) throw new Exception("Client not found!");

            _Db.ClientModels.Remove(client);
            await _Db.SaveChangesAsync();
        }
    }
}