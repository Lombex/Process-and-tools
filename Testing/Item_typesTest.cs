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
    public class ItemTypeIntegrationTest
    {
        private string _jsonFilePath = "data/item_type.json";
        private List<ItemTypeModel> ReadItemTypesFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
            var json = File.ReadAllText(_jsonFilePath);
            return JsonConvert.DeserializeObject<List<ItemTypeModel>>(json) ?? new List<ItemTypeModel>();
        }

        private void WriteItemTypesToJson(List<ItemTypeModel> itemTypes)
        {
            var json = JsonConvert.SerializeObject(itemTypes, Formatting.Indented);
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

            // Initialize ItemType data for testing with sample data
            var itemTypeList = new List<ItemTypeModel>
            {
                new ItemTypeModel
                {
                    id = 1,
                    name = "Electronics",
                    description = "Electronic devices and accessories",
                    created_at = DateTime.Parse("2020-01-01 12:00:00"),
                    updated_at = DateTime.Parse("2020-01-01 12:00:00")
                }
            };

            WriteItemTypesToJson(itemTypeList);
        }

        // Test: Add ItemType
        [TestMethod]
        public void Test_Add_ItemType()
        {
            var itemType = new ItemTypeModel
            {
                id = 2,
                name = "Furniture",
                description = "Furniture items for home and office",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var itemTypeList = ReadItemTypesFromJson();
            itemTypeList.Add(itemType);
            WriteItemTypesToJson(itemTypeList);

            var savedItemType = itemTypeList.FirstOrDefault(i => i.id == itemType.id);
            Assert.IsNotNull(savedItemType);
            Assert.AreEqual("Furniture", savedItemType?.name);
            Assert.AreEqual("Furniture items for home and office", savedItemType?.description);
        }

        // Test: Get ItemType by ID
        [TestMethod]
        public void Test_Get_ItemType()
        {
            var itemTypeId = 1;  // Assuming the ID of the item type you want to fetch is 1

            var itemTypeList = ReadItemTypesFromJson();
            var fetchedItemType = itemTypeList.FirstOrDefault(i => i.id == itemTypeId);  // Search by ID

            Assert.IsNotNull(fetchedItemType);
            Assert.AreEqual("Electronics", fetchedItemType?.name);
            Assert.AreEqual("Electronic devices and accessories", fetchedItemType?.description);
        }

        // Test: Update ItemType
        [TestMethod]
        public void Test_Update_ItemType()
        {
            var itemTypeId = 1;
            var updatedDescription = "Updated description for Electronics";

            var itemTypeList = ReadItemTypesFromJson();
            var itemTypeToUpdate = itemTypeList.FirstOrDefault(i => i.id == itemTypeId);
            if (itemTypeToUpdate != null)
            {
                itemTypeToUpdate.description = updatedDescription;
                itemTypeToUpdate.updated_at = DateTime.Now;
                WriteItemTypesToJson(itemTypeList);
            }

            var updatedItemType = itemTypeList.FirstOrDefault(i => i.id == itemTypeId);
            Assert.IsNotNull(updatedItemType);
            Assert.AreEqual(updatedDescription, updatedItemType?.description);
        }

        // Test: Delete ItemType
        [TestMethod]
        public void Test_Delete_ItemType()
        {
            var itemType = new ItemTypeModel
            {
                id = 3,
                name = "Clothing",
                description = "Apparel and accessories",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var itemTypeList = ReadItemTypesFromJson();
            itemTypeList.Add(itemType);
            WriteItemTypesToJson(itemTypeList);

            itemTypeList.Remove(itemType);
            WriteItemTypesToJson(itemTypeList);

            var deletedItemType = itemTypeList.FirstOrDefault(i => i.id == itemType.id);
            Assert.IsNull(deletedItemType);
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
