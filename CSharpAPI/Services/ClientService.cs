using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpAPI.Models;

namespace CSharpAPI.Services
{
    public interface IClientsService
    {
        IEnumerable<ClientModel> GetAllClients();
        ClientModel GetClientById(int id);
        void CreateClient(ClientModel client);
        void UpdateClient(ClientModel client);
        void DeleteClient(int id);
    }

    public class ClientsService : IClientsService
    {
        private readonly string dataFolder;

        public ClientsService(string jsonFilePath = "data/clients.json")
        {
            dataFolder = jsonFilePath;
        }

        public IEnumerable<ClientModel> GetAllClients()
        {
            if (!File.Exists(dataFolder))
                return new List<ClientModel>();

            var jsonContent = File.ReadAllText(dataFolder);
            return JsonConvert.DeserializeObject<List<ClientModel>>(jsonContent) ?? new List<ClientModel>();
        }

        public ClientModel GetClientById(int id)
        {
            return GetAllClients().FirstOrDefault(c => c.id == id);
        }

        public void CreateClient(ClientModel client)
        {
            var clients = GetAllClients().ToList();
            client.id = clients.Count > 0 ? clients.Max(c => c.id) + 1 : 1;
            client.created_at = DateTime.Now;
            client.updated_at = DateTime.Now;
            clients.Add(client);
            SaveClients(clients);
        }

        public void UpdateClient(ClientModel client)
        {
            var clients = GetAllClients().ToList();
            var existingClient = clients.FirstOrDefault(c => c.id == client.id);
            if (existingClient != null)
            {
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
                SaveClients(clients);
            }
        }

        public void DeleteClient(int id)
        {
            var clients = GetAllClients().ToList();
            var clientToDelete = clients.FirstOrDefault(c => c.id == id);
            if (clientToDelete != null)
            {
                clients.Remove(clientToDelete);
                SaveClients(clients);
            }
        }

        private void SaveClients(List<ClientModel> clients)
        {
            var jsonContent = JsonConvert.SerializeObject(clients, Formatting.Indented);
            File.WriteAllText(dataFolder, jsonContent);
        }
    }
}