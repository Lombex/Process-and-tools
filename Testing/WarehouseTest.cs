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
    public class WarehouseIntegrationTest
    {
        private string _jsonFilePath = "data/warehouse.json";

        private List<WarehouseModel> ReadWarehousesFromJson()
        {
            if (!File.Exists(_jsonFilePath)) File.WriteAllText(_jsonFilePath, "[]");
            return JsonConvert.DeserializeObject<List<WarehouseModel>>(File.ReadAllText(_jsonFilePath)) ?? new List<WarehouseModel>();
        }

        private void WriteWarehousesToJson(List<WarehouseModel> warehouses) =>
            File.WriteAllText(_jsonFilePath, JsonConvert.SerializeObject(warehouses, Formatting.Indented));

        [TestInitialize]
        public void TestInitialize()
        {
            var warehouseList = new List<WarehouseModel>
            {
                new WarehouseModel
                {
                    id = 1, code = "WH001", name = "Main Warehouse", address = "123 Main St", zip = "12345",
                    city = "Anytown", province = "CA", country = "USA",
                    contact = new Contact { name = "John Doe", phone = "123-456-7890", email = "john.doe@example.com" },
                    created_at = DateTime.Now, updated_at = DateTime.Now
                }
            };

            WriteWarehousesToJson(warehouseList);
        }

        [TestMethod]
        public void Test_Add_Warehouse()
        {
            var warehouse = new WarehouseModel
            {
                id = 2, code = "WH002", name = "Secondary Warehouse", address = "456 Secondary St", zip = "67890",
                city = "Othertown", province = "TX", country = "USA",
                contact = new Contact { name = "Jane Smith", phone = "987-654-3210", email = "jane.smith@example.com" },
                created_at = DateTime.Now, updated_at = DateTime.Now
            };

            var warehouseList = ReadWarehousesFromJson();
            warehouseList.Add(warehouse);
            WriteWarehousesToJson(warehouseList);

            var savedWarehouse = warehouseList.FirstOrDefault(w => w.id == warehouse.id);
            Assert.IsNotNull(savedWarehouse);
            Assert.AreEqual("WH002", savedWarehouse?.code);
        }

        [TestMethod]
        public void Test_Get_Warehouse()
        {
            var warehouseList = ReadWarehousesFromJson();
            var warehouse = warehouseList.First(w => w.id == 1);

            Assert.AreEqual("Main Warehouse", warehouse?.name);
            Assert.AreEqual("12345", warehouse?.zip);
        }

        [TestMethod]
        public void Test_Update_Warehouse()
        {
            var warehouseList = ReadWarehousesFromJson();
            var warehouse = warehouseList.First(w => w.id == 1);

            warehouse.name = "Updated Warehouse Name";
            warehouse.updated_at = DateTime.Now;

            WriteWarehousesToJson(warehouseList);

            var updatedWarehouse = ReadWarehousesFromJson().First(w => w.id == 1);
            Assert.AreEqual("Updated Warehouse Name", updatedWarehouse?.name);
        }

        [TestMethod]
        public void Test_Delete_Warehouse()
        {
            var warehouse = new WarehouseModel
            {
                id = 3, code = "WH003", name = "Warehouse 3", address = "789 Test St", zip = "11223", city = "TestCity",
                province = "TestProvince", country = "TestCountry", contact = new Contact { name = "Mary Jane", phone = "555-555-5555", email = "mary.jane@example.com" },
                created_at = DateTime.Now, updated_at = DateTime.Now
            };

            var warehouseList = ReadWarehousesFromJson();
            warehouseList.Add(warehouse);
            WriteWarehousesToJson(warehouseList);

            warehouseList.Remove(warehouse);
            WriteWarehousesToJson(warehouseList);

            Assert.IsNull(ReadWarehousesFromJson().FirstOrDefault(w => w.id == warehouse.id));
        }

        [TestMethod]
        public void Test_Empty_Warehouse_List()
        {
            var emptyList = new List<WarehouseModel>();
            WriteWarehousesToJson(emptyList);

            var warehouseList = ReadWarehousesFromJson();
            Assert.AreEqual(0, warehouseList.Count);
        }

        // New Test: Ensure Warehouse Can Be Found by Code
        [TestMethod]
        public void Test_Get_Warehouse_By_Code()
        {
            var warehouseList = ReadWarehousesFromJson();
            var warehouse = warehouseList.FirstOrDefault(w => w.code == "WH001");

            Assert.IsNotNull(warehouse);
            Assert.AreEqual("Main Warehouse", warehouse?.name);
            Assert.AreEqual("12345", warehouse?.zip);
        }

        // New Test: Ensure Non-Existent Warehouse Returns Null
        [TestMethod]
        public void Test_Get_Non_Existent_Warehouse()
        {
            var warehouseList = ReadWarehousesFromJson();
            var warehouse = warehouseList.FirstOrDefault(w => w.id == 9999); 

            Assert.IsNull(warehouse);
        }

        // New Test: Update Multiple Fields of Warehouse
        [TestMethod]
        public void Test_Update_Multiple_Warehouse_Fields()
        {
            var warehouseList = ReadWarehousesFromJson();
            var warehouse = warehouseList.First(w => w.id == 1);

            warehouse.name = "Updated Main Warehouse Name";
            warehouse.address = "456 Updated St";
            warehouse.updated_at = DateTime.Now;

            WriteWarehousesToJson(warehouseList);

            var updatedWarehouse = ReadWarehousesFromJson().First(w => w.id == 1);
            Assert.AreEqual("Updated Main Warehouse Name", updatedWarehouse?.name);
            Assert.AreEqual("456 Updated St", updatedWarehouse?.address);
        }
       [TestMethod]
        public void Test_Get_Warehouse_With_Invalid_Id()
        {
            var warehouseList = ReadWarehousesFromJson();
            var warehouse = warehouseList.FirstOrDefault(w => w.id == 9999);

            Assert.IsNull(warehouse);
        }

        [TestCleanup]
        public void TestCleanup() => File.Delete(_jsonFilePath);
    }
}
