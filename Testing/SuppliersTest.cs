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
    public class SupplierIntegrationTest
    {
        private string _jsonFilePath = "data/supplier.json";

        private List<SupplierModel> ReadSuppliersFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
            var json = File.ReadAllText(_jsonFilePath);

            var supplierList = JsonConvert.DeserializeObject<List<SupplierModel>>(json);

            return supplierList ?? new List<SupplierModel>();
        }

        private void WriteSuppliersToJson(List<SupplierModel> suppliers)
        {
            var json = JsonConvert.SerializeObject(suppliers, Formatting.Indented);
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

            // Initialize supplier data for testing
            var supplierList = new List<SupplierModel>
            {
                new SupplierModel
                {
                    id = 1,
                    code = "SUP0001",
                    name = "Lee, Parks and Johnson",
                    address = "5989 Sullivan Drives",
                    address_extra = "Apt. 996",
                    city = "Port Anitaburgh",
                    zip_code = "91688",
                    province = "Illinois",
                    country = "Czech Republic",
                    contact_name = "Toni Barnett",
                    phonenumber = "363.541.7282x36825",
                    reference = "LPaJ-SUP0001",
                    created_at = DateTime.Parse("1971-10-20 18:06:17"),
                    updated_at = DateTime.Parse("1985-06-08 00:13:46")
                }
            };

            WriteSuppliersToJson(supplierList);
        }

        // Test: Add Supplier
        [TestMethod]
        public void Test_Add_Supplier()
        {
            var supplier = new SupplierModel
            {
                id = 2,
                code = "SUP0002",
                name = "Acme Supplies",
                address = "123 Industrial Rd",
                address_extra = "Suite 2B",
                city = "New York",
                zip_code = "10001",
                province = "NY",
                country = "USA",
                contact_name = "Jane Doe",
                phonenumber = "555-123-4567",
                reference = "Acme-SUP0002",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var supplierList = ReadSuppliersFromJson();
            supplierList.Add(supplier);
            WriteSuppliersToJson(supplierList);

            var savedSupplier = supplierList.FirstOrDefault(s => s.id == supplier.id);
            Assert.IsNotNull(savedSupplier);
            Assert.AreEqual("Acme Supplies", savedSupplier?.name);
            Assert.AreEqual("10001", savedSupplier?.zip_code);
        }

        // Test: Get Supplier by ID
        [TestMethod]
        public void Test_Get_Supplier()
        {
            var supplierId = 1; 

            var supplierList = ReadSuppliersFromJson();
            var fetchedSupplier = supplierList.FirstOrDefault(s => s.id == supplierId);

            Assert.IsNotNull(fetchedSupplier);
            Assert.AreEqual("Lee, Parks and Johnson", fetchedSupplier?.name);
            Assert.AreEqual("91688", fetchedSupplier?.zip_code);
        }

        // Test: Delete Supplier
        [TestMethod]
        public void Test_Delete_Supplier()
        {
            var supplier = new SupplierModel
            {
                id = 3,
                code = "SUP0003",
                name = "Global Corp",
                address = "789 Tech Blvd",
                address_extra = "Unit 301",
                city = "Silicon Valley",
                zip_code = "94043",
                province = "CA",
                country = "USA",
                contact_name = "Mark Smith",
                phonenumber = "555-987-6543",
                reference = "GlobalCorp-SUP0003",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };
            var supplierList = ReadSuppliersFromJson();
            supplierList.Add(supplier);
            WriteSuppliersToJson(supplierList);

            supplierList.Remove(supplier);
            WriteSuppliersToJson(supplierList);

            var deletedSupplier = supplierList.FirstOrDefault(s => s.id == supplier.id);
            Assert.IsNull(deletedSupplier);
        }

        // Test: Update Supplier
        [TestMethod]
        public void Test_Update_Supplier()
        {
            var supplierId = 1;
            var updatedName = "Updated Supplier One";

            var supplierList = ReadSuppliersFromJson();
            var supplierToUpdate = supplierList.FirstOrDefault(s => s.id == supplierId); 
            if (supplierToUpdate != null)
            {
                supplierToUpdate.name = updatedName;
                supplierToUpdate.updated_at = DateTime.Now;
                WriteSuppliersToJson(supplierList);
            }

            var updatedSupplier = supplierList.FirstOrDefault(s => s.id == supplierId); 
            Assert.IsNotNull(updatedSupplier);
            Assert.AreEqual(updatedName, updatedSupplier?.name);
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
