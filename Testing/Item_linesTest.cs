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
    public class ItemLineIntegrationTest
    {
        private string _jsonFilePath = "data/item_line.json";

        private List<ItemLineModel> ReadItemLinesFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
            var json = File.ReadAllText(_jsonFilePath);
            return JsonConvert.DeserializeObject<List<ItemLineModel>>(json) ?? new List<ItemLineModel>();
        }

        private void WriteItemLinesToJson(List<ItemLineModel> itemLines)
        {
            var json = JsonConvert.SerializeObject(itemLines, Formatting.Indented);
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

            // Initialize ItemLine data for testing with sample data
            var itemLineList = new List<ItemLineModel>
            {
                new ItemLineModel
                {
                    id = 1,
                    name = "Item Line 1",
                    description = "Description of Item Line 1",
                    itemgroup_id = 101,
                    created_at = DateTime.Parse("2020-01-01 12:00:00"),
                    updated_at = DateTime.Parse("2020-01-01 12:00:00")
                }
            };

            WriteItemLinesToJson(itemLineList);
        }

        // Test: Add ItemLine
        [TestMethod]
        public void Test_Add_ItemLine()
        {
            var itemLine = new ItemLineModel
            {
                id = 2,
                name = "Item Line 2",
                description = "Description of Item Line 2",
                itemgroup_id = 102,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var itemLineList = ReadItemLinesFromJson();
            itemLineList.Add(itemLine);
            WriteItemLinesToJson(itemLineList);

            var savedItemLine = itemLineList.FirstOrDefault(i => i.id == itemLine.id);
            Assert.IsNotNull(savedItemLine);
            Assert.AreEqual("Item Line 2", savedItemLine?.name);
            Assert.AreEqual("Description of Item Line 2", savedItemLine?.description);
        }

        // Test: Get ItemLine by ID
        [TestMethod]
        public void Test_Get_ItemLine()
        {
            var itemLineId = 1;  // Assuming the ID of the item line you want to fetch is 1

            var itemLineList = ReadItemLinesFromJson();
            var fetchedItemLine = itemLineList.FirstOrDefault(i => i.id == itemLineId);  // Search by ID

            Assert.IsNotNull(fetchedItemLine);
            Assert.AreEqual("Item Line 1", fetchedItemLine?.name);
            Assert.AreEqual("Description of Item Line 1", fetchedItemLine?.description);
        }

        // Test: Update ItemLine
        [TestMethod]
        public void Test_Update_ItemLine()
        {
            var itemLineId = 1;
            var updatedDescription = "Updated description for Item Line 1";

            var itemLineList = ReadItemLinesFromJson();
            var itemLineToUpdate = itemLineList.FirstOrDefault(i => i.id == itemLineId);
            if (itemLineToUpdate != null)
            {
                itemLineToUpdate.description = updatedDescription;
                itemLineToUpdate.updated_at = DateTime.Now;
                WriteItemLinesToJson(itemLineList);
            }

            var updatedItemLine = itemLineList.FirstOrDefault(i => i.id == itemLineId);
            Assert.IsNotNull(updatedItemLine);
            Assert.AreEqual(updatedDescription, updatedItemLine?.description);
        }

        // Test: Delete ItemLine
        [TestMethod]
        public void Test_Delete_ItemLine()
        {
            var itemLine = new ItemLineModel
            {
                id = 3,
                name = "Item Line 3",
                description = "Description of Item Line 3",
                itemgroup_id = 103,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var itemLineList = ReadItemLinesFromJson();
            itemLineList.Add(itemLine);
            WriteItemLinesToJson(itemLineList);

            itemLineList.Remove(itemLine);
            WriteItemLinesToJson(itemLineList);

            var deletedItemLine = itemLineList.FirstOrDefault(i => i.id == itemLine.id);
            Assert.IsNull(deletedItemLine);
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
