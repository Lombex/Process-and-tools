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
    public class ItemGroupIntegrationTest
    {
        private string _jsonFilePath = "data/item_group.json";

        private List<ItemGroupModel> ReadItemGroupsFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
            var json = File.ReadAllText(_jsonFilePath);
            return JsonConvert.DeserializeObject<List<ItemGroupModel>>(json) ?? new List<ItemGroupModel>();
        }

        private void WriteItemGroupsToJson(List<ItemGroupModel> itemGroups)
        {
            var json = JsonConvert.SerializeObject(itemGroups, Formatting.Indented);
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

            // Initialize ItemGroup data for testing with the provided real data
            var itemGroupList = new List<ItemGroupModel>
            {
                new ItemGroupModel
                {
                    id = 1,
                    name = "Furniture",
                    description = "",
                    created_at = DateTime.Parse("2019-09-22 15:51:07"),
                    updated_at = DateTime.Parse("2022-05-18 13:49:28")
                }
            };

            WriteItemGroupsToJson(itemGroupList);
        }

        // Test: Add ItemGroup
        [TestMethod]
        public void Test_Add_ItemGroup()
        {
            var itemGroup = new ItemGroupModel
            {
                id = 2,
                name = "Appliances",
                description = "Home appliances and gadgets",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var itemGroupList = ReadItemGroupsFromJson();
            itemGroupList.Add(itemGroup);
            WriteItemGroupsToJson(itemGroupList);

            var savedItemGroup = itemGroupList.FirstOrDefault(i => i.id == itemGroup.id);
            Assert.IsNotNull(savedItemGroup);
            Assert.AreEqual("Appliances", savedItemGroup?.name);
            Assert.AreEqual("Home appliances and gadgets", savedItemGroup?.description);
        }

        // Test: Get ItemGroup by ID
        [TestMethod]
        public void Test_Get_ItemGroup()
        {
            var itemGroupId = 1;  // Assuming the ID of the item group you want to fetch is 1

            var itemGroupList = ReadItemGroupsFromJson();
            var fetchedItemGroup = itemGroupList.FirstOrDefault(i => i.id == itemGroupId);  // Search by ID

            Assert.IsNotNull(fetchedItemGroup);
            Assert.AreEqual("Furniture", fetchedItemGroup?.name);
            Assert.AreEqual("", fetchedItemGroup?.description);  // Empty description as per the real data
        }

        // Test: Update ItemGroup
        [TestMethod]
        public void Test_Update_ItemGroup()
        {
            var itemGroupId = 1;
            var updatedDescription = "Updated description for Furniture";

            var itemGroupList = ReadItemGroupsFromJson();
            var itemGroupToUpdate = itemGroupList.FirstOrDefault(i => i.id == itemGroupId);
            if (itemGroupToUpdate != null)
            {
                itemGroupToUpdate.description = updatedDescription;
                itemGroupToUpdate.updated_at = DateTime.Now;
                WriteItemGroupsToJson(itemGroupList);
            }

            var updatedItemGroup = itemGroupList.FirstOrDefault(i => i.id == itemGroupId);
            Assert.IsNotNull(updatedItemGroup);
            Assert.AreEqual(updatedDescription, updatedItemGroup?.description);
        }

        // Test: Delete ItemGroup
        [TestMethod]
        public void Test_Delete_ItemGroup()
        {
            var itemGroup = new ItemGroupModel
            {
                id = 3,
                name = "Toys",
                description = "Toys and games for children",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var itemGroupList = ReadItemGroupsFromJson();
            itemGroupList.Add(itemGroup);
            WriteItemGroupsToJson(itemGroupList);

            itemGroupList.Remove(itemGroup);
            WriteItemGroupsToJson(itemGroupList);

            var deletedItemGroup = itemGroupList.FirstOrDefault(i => i.id == itemGroup.id);
            Assert.IsNull(deletedItemGroup);
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
