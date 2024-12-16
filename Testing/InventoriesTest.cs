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
    public class InventorieIntegrationTest
    {
        private string _jsonFilePath = "data/inventories.json";

        private List<InventorieModel> ReadInventoriesFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
            var json = File.ReadAllText(_jsonFilePath);
            return JsonConvert.DeserializeObject<List<InventorieModel>>(json) ?? new List<InventorieModel>();
        }

        private void WriteInventoriesToJson(List<InventorieModel> inventories)
        {
            var json = JsonConvert.SerializeObject(inventories, Formatting.Indented);
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

            // Initialize Inventorie data for testing with the provided real data
            var inventorieList = new List<InventorieModel>
            {
                new InventorieModel
                {
                    id = 1,
                    item_id = "P000001",
                    description = "Face-to-face clear-thinking complexity",
                    item_reference = "sjQ23408K",
                    locations = new List<int> { 3211, 24700, 14123, 19538, 31071, 24701, 11606, 11817 },
                    total_on_hand = 262,
                    total_expected = 0,
                    total_ordered = 80,
                    total_allocated = 41,
                    total_available = 141,
                    created_at = DateTime.Parse("2015-02-19 16:08:24"),
                    updated_at = DateTime.Parse("2015-09-26 06:37:56")
                }
            };

            WriteInventoriesToJson(inventorieList);
        }

        // Test: Add Inventorie
        [TestMethod]
        public void Test_Add_Inventorie()
        {
            var inventorie = new InventorieModel
            {
                id = 2,
                item_id = "P000002",
                description = "Innovative product for inventory",
                item_reference = "abC23409L",
                locations = new List<int> { 1122, 2244, 5566 },
                total_on_hand = 50,
                total_expected = 10,
                total_ordered = 30,
                total_allocated = 15,
                total_available = 35,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var inventorieList = ReadInventoriesFromJson();
            inventorieList.Add(inventorie);
            WriteInventoriesToJson(inventorieList);

            var savedInventorie = inventorieList.FirstOrDefault(i => i.id == inventorie.id);
            Assert.IsNotNull(savedInventorie);
            Assert.AreEqual("P000002", savedInventorie?.item_id);
            Assert.AreEqual("Innovative product for inventory", savedInventorie?.description);
        }

        // Test: Get Inventorie by ID
        [TestMethod]
        public void Test_Get_Inventorie()
        {
            var inventorieId = 1;  // Assuming the ID of the inventory you want to fetch is 1

            var inventorieList = ReadInventoriesFromJson();
            var fetchedInventorie = inventorieList.FirstOrDefault(i => i.id == inventorieId);  // Search by ID

            Assert.IsNotNull(fetchedInventorie);
            Assert.AreEqual("P000001", fetchedInventorie?.item_id);
            Assert.AreEqual("Face-to-face clear-thinking complexity", fetchedInventorie?.description);
        }

        // Test: Update Inventorie
        [TestMethod]
        public void Test_Update_Inventorie()
        {
            var inventorieId = 1;
            var updatedDescription = "Updated description for inventory item P000001";

            var inventorieList = ReadInventoriesFromJson();
            var inventorieToUpdate = inventorieList.FirstOrDefault(i => i.id == inventorieId);
            if (inventorieToUpdate != null)
            {
                inventorieToUpdate.description = updatedDescription;
                inventorieToUpdate.updated_at = DateTime.Now;
                WriteInventoriesToJson(inventorieList);
            }

            var updatedInventorie = inventorieList.FirstOrDefault(i => i.id == inventorieId);
            Assert.IsNotNull(updatedInventorie);
            Assert.AreEqual(updatedDescription, updatedInventorie?.description);
        }

        // Test: Delete Inventorie
        [TestMethod]
        public void Test_Delete_Inventorie()
        {
            var inventorie = new InventorieModel
            {
                id = 3,
                item_id = "P000003",
                description = "Sample item for deletion",
                item_reference = "xyZ23409M",
                locations = new List<int> { 5555, 6666 },
                total_on_hand = 20,
                total_expected = 5,
                total_ordered = 10,
                total_allocated = 8,
                total_available = 12,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var inventorieList = ReadInventoriesFromJson();
            inventorieList.Add(inventorie);
            WriteInventoriesToJson(inventorieList);

            inventorieList.Remove(inventorie);
            WriteInventoriesToJson(inventorieList);

            var deletedInventorie = inventorieList.FirstOrDefault(i => i.id == inventorie.id);
            Assert.IsNull(deletedInventorie);
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
