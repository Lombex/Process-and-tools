using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpAPI.Models;

namespace CSharpAPI.Tests
{
    [TestClass]
    public class ClientIntegrationTest
    {
        private string _jsonFilePath = "data/clients.json";

        private List<ClientModel> ReadClientsFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
            var json = File.ReadAllText(_jsonFilePath);
            return JsonConvert.DeserializeObject<List<ClientModel>>(json) ?? new List<ClientModel>();
        }

        private void WriteClientsToJson(List<ClientModel> clients)
        {
            var json = JsonConvert.SerializeObject(clients, Formatting.Indented);
            File.WriteAllText(_jsonFilePath, json);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            var directory = Path.GetDirectoryName(_jsonFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var clientList = new List<ClientModel>
            {
                new ClientModel
                {
                    id = 1,
                    name = "Raymond Inc",
                    address = "1296 Daniel Road Apt. 349",
                    city = "Pierceview",
                    zip_code = "28301",
                    province = "Colorado",
                    country = "United States",
                    contact_name = "Bryan Clark",
                    contact_phone = "242.732.3483x2573",
                    contact_email = "robertcharles@example.net",
                    created_at = DateTime.Parse("2010-04-28 02:22:53"),
                    updated_at = DateTime.Parse("2022-02-09 20:22:35")
                }
            };

            WriteClientsToJson(clientList);
        }

        // Test: Add Client
        [TestMethod]
        public void Test_Add_Client()
        {
            var client = new ClientModel
            {
                id = 2,
                name = "Tech Solutions",
                address = "789 Tech Park",
                city = "Silicon Valley",
                zip_code = "94016",
                province = "California",
                country = "United States",
                contact_name = "Jane Smith",
                contact_phone = "555-987-6543",
                contact_email = "janesmith@techsolutions.com",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var clientList = ReadClientsFromJson();
            clientList.Add(client);
            WriteClientsToJson(clientList);

            var savedClient = clientList.FirstOrDefault(c => c.id == client.id);
            Assert.IsNotNull(savedClient);
            Assert.AreEqual("Tech Solutions", savedClient?.name);
            Assert.AreEqual("janesmith@techsolutions.com", savedClient?.contact_email);
        }

        // Test: Get Client by ID
        [TestMethod]
        public void Test_Get_Client()
        {
            var clientId = 1;  // Assuming the ID of the client you want to fetch is 1

            var clientList = ReadClientsFromJson();
            var fetchedClient = clientList.FirstOrDefault(c => c.id == clientId);  // Search by ID

            Assert.IsNotNull(fetchedClient);
            Assert.AreEqual("Raymond Inc", fetchedClient?.name);
            Assert.AreEqual("242.732.3483x2573", fetchedClient?.contact_phone);
        }

        // Test: Update Client
        [TestMethod]
        public void Test_Update_Client()
        {
            var clientId = 1;
            var updatedContactPhone = "555-000-1234";

            var clientList = ReadClientsFromJson();
            var clientToUpdate = clientList.FirstOrDefault(c => c.id == clientId);
            if (clientToUpdate != null)
            {
                clientToUpdate.contact_phone = updatedContactPhone;
                clientToUpdate.updated_at = DateTime.Now;
                WriteClientsToJson(clientList);
            }

            var updatedClient = clientList.FirstOrDefault(c => c.id == clientId);
            Assert.IsNotNull(updatedClient);
            Assert.AreEqual(updatedContactPhone, updatedClient?.contact_phone);
        }

        // Test: Delete Client
        [TestMethod]
        public void Test_Delete_Client()
        {
            var client = new ClientModel
            {
                id = 3,
                name = "Gamma Enterprises",
                address = "123 Gamma St",
                city = "Star City",
                zip_code = "12345",
                province = "Illinois",
                country = "USA",
                contact_name = "Alice Cooper",
                contact_phone = "555-111-2222",
                contact_email = "alicecooper@gamma.com",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var clientList = ReadClientsFromJson();
            clientList.Add(client);
            WriteClientsToJson(clientList);

            clientList.Remove(client);
            WriteClientsToJson(clientList);

            var deletedClient = clientList.FirstOrDefault(c => c.id == client.id);
            Assert.IsNull(deletedClient);
        }

        // Clean up the JSON file after the tests
        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(_jsonFilePath))
            {
                File.Delete(_jsonFilePath);
            }
        }
    }
}
