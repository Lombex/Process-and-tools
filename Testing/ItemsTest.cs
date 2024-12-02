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
    public class ItemIntegrationTest
    {
        private string _jsonFilePath = "data/items.json";

        private List<ItemModel> ReadItemsFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
            var json = File.ReadAllText(_jsonFilePath);
            return JsonConvert.DeserializeObject<List<ItemModel>>(json) ?? new List<ItemModel>();
        }

        private void WriteItemsToJson(List<ItemModel> items)
        {
            var json = JsonConvert.SerializeObject(items, Formatting.Indented);
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

            // Initialize Item data for testing with the provided real data
            var itemList = new List<ItemModel>
            {
                new ItemModel
                {
                    uid = "P000001",
                    code = "sjQ23408K",
                    description = "Face-to-face clear-thinking complexity",
                    short_description = "must",
                    upc_code = "6523540947122",
                    model_number = "63-OFFTq0T",
                    commodity_code = "oTo304",
                    item_line = 11,
                    item_group = 73,
                    item_type = 14,
                    unit_purchase_quantity = 47,
                    unit_order_quantity = 13,
                    pack_order_quantity = 11,
                    supplier_id = 34,
                    supplier_code = "SUP423",
                    supplier_part_number = "E-86805-uTM",
                    created_at = DateTime.Parse("2015-02-19 16:08:24"),
                    updated_at = DateTime.Parse("2015-09-26 06:37:56")
                }
            };

            WriteItemsToJson(itemList);
        }

        // Test: Add Item
        [TestMethod]
        public void Test_Add_Item()
        {
            var item = new ItemModel
            {
                uid = "P000002",
                code = "abC23409L",
                description = "Innovative product",
                short_description = "necessary",
                upc_code = "6523540947123",
                model_number = "64-OFFTq1T",
                commodity_code = "oTo305",
                item_line = 12,
                item_group = 74,
                item_type = 15,
                unit_purchase_quantity = 48,
                unit_order_quantity = 14,
                pack_order_quantity = 12,
                supplier_id = 35,
                supplier_code = "SUP424",
                supplier_part_number = "E-86806-uTM",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var itemList = ReadItemsFromJson();
            itemList.Add(item);
            WriteItemsToJson(itemList);

            var savedItem = itemList.FirstOrDefault(i => i.uid == item.uid);
            Assert.IsNotNull(savedItem);
            Assert.AreEqual("P000002", savedItem?.uid);
            Assert.AreEqual("Innovative product", savedItem?.description);
        }

        // Test: Get Item by UID
        [TestMethod]
        public void Test_Get_Item()
        {
            var itemUid = "P000001";  // Assuming the UID of the item you want to fetch is P000001

            var itemList = ReadItemsFromJson();
            var fetchedItem = itemList.FirstOrDefault(i => i.uid == itemUid);  // Search by UID

            Assert.IsNotNull(fetchedItem);
            Assert.AreEqual("sjQ23408K", fetchedItem?.code);
            Assert.AreEqual("Face-to-face clear-thinking complexity", fetchedItem?.description);
        }

        // Test: Update Item
        [TestMethod]
        public void Test_Update_Item()
        {
            var itemUid = "P000001";
            var updatedDescription = "Updated description for item P000001";

            var itemList = ReadItemsFromJson();
            var itemToUpdate = itemList.FirstOrDefault(i => i.uid == itemUid);
            if (itemToUpdate != null)
            {
                itemToUpdate.description = updatedDescription;
                itemToUpdate.updated_at = DateTime.Now;
                WriteItemsToJson(itemList);
            }

            var updatedItem = itemList.FirstOrDefault(i => i.uid == itemUid);
            Assert.IsNotNull(updatedItem);
            Assert.AreEqual(updatedDescription, updatedItem?.description);
        }

        // Test: Delete Item
        [TestMethod]
        public void Test_Delete_Item()
        {
            var item = new ItemModel
            {
                uid = "P000003",
                code = "xyZ23409M",
                description = "Sample item for deletion",
                short_description = "temporary",
                upc_code = "6523540947124",
                model_number = "65-OFFTq2T",
                commodity_code = "oTo306",
                item_line = 13,
                item_group = 75,
                item_type = 16,
                unit_purchase_quantity = 49,
                unit_order_quantity = 15,
                pack_order_quantity = 13,
                supplier_id = 36,
                supplier_code = "SUP425",
                supplier_part_number = "E-86807-uTM",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var itemList = ReadItemsFromJson();
            itemList.Add(item);
            WriteItemsToJson(itemList);

            itemList.Remove(item);
            WriteItemsToJson(itemList);

            var deletedItem = itemList.FirstOrDefault(i => i.uid == item.uid);
            Assert.IsNull(deletedItem);
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
