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
    public class ShipmentIntegrationTest
    {
        private string _jsonFilePath = "data/shipment.json";

        private List<ShipmentModel> ReadShipmentsFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
            var json = File.ReadAllText(_jsonFilePath);
            return JsonConvert.DeserializeObject<List<ShipmentModel>>(json) ?? new List<ShipmentModel>();
        }

        private void WriteShipmentsToJson(List<ShipmentModel> shipments)
        {
            var json = JsonConvert.SerializeObject(shipments, Formatting.Indented);
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

            var shipmentList = new List<ShipmentModel>
            {
                new ShipmentModel
                {
                    id = 1,
                    order_id = 1,
                    source_id = 33,
                    order_date = "2000-03-09",
                    request_date = "2000-03-11",
                    shipment_date = "2000-03-13",
                    shipment_type = "I",
                    shipment_status = "Pending",
                    notes = "Zee vertrouwen klas rots heet lachen oneven begrijpen.",
                    carrier_code = "DPD",
                    carrier_description = "Dynamic Parcel Distribution",
                    service_code = "Fastest",
                    payment_type = "Manual",
                    transfer_mode = "Ground",
                    total_package_count = 31,
                    total_package_weight = 594.42f,
                    created_at = DateTime.Parse("2000-03-10T11:11:14Z"),
                    updated_at = DateTime.Parse("2000-03-11T13:11:14Z"),
                    items = new List<Items>
                    {
                        new Items { item_id = "P007435", amount = 23 },
                        new Items { item_id = "P009557", amount = 1 },
                        new Items { item_id = "P009553", amount = 50 },
                        new Items { item_id = "P010015", amount = 16 },
                        new Items { item_id = "P002084", amount = 33 },
                        new Items { item_id = "P009663", amount = 18 },
                        new Items { item_id = "P010125", amount = 18 },
                        new Items { item_id = "P005768", amount = 26 },
                        new Items { item_id = "P004051", amount = 1 },
                        new Items { item_id = "P005026", amount = 29 },
                        new Items { item_id = "P000726", amount = 22 },
                        new Items { item_id = "P008107", amount = 47 },
                        new Items { item_id = "P001598", amount = 32 },
                        new Items { item_id = "P002855", amount = 20 },
                        new Items { item_id = "P010404", amount = 30 },
                        new Items { item_id = "P010446", amount = 6 },
                        new Items { item_id = "P001517", amount = 9 },
                        new Items { item_id = "P009265", amount = 2 },
                        new Items { item_id = "P001108", amount = 20 },
                        new Items { item_id = "P009110", amount = 18 },
                        new Items { item_id = "P009686", amount = 13 }
                    }
                }
            };

            WriteShipmentsToJson(shipmentList);
        }

        // Test: Add Shipment
        [TestMethod]
        public void Test_Add_Shipment()
        {
            var shipment = new ShipmentModel
            {
                id = 2,
                order_id = 1002,
                source_id = 2,
                order_date = "2024-11-05",
                request_date = "2024-11-06",
                shipment_date = "2024-11-07",
                shipment_type = "Express",
                shipment_status = "In Transit",
                notes = "Urgent delivery",
                carrier_code = "FedEx",
                carrier_description = "Federal Express",
                service_code = "FedEx_Express",
                payment_type = "Collect",
                transfer_mode = "Ground",
                total_package_count = 3,
                total_package_weight = 7.8f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { item_id = "P012345", amount = 10 }
                }
            };

            var shipmentList = ReadShipmentsFromJson();
            shipmentList.Add(shipment);
            WriteShipmentsToJson(shipmentList);

            var savedShipment = shipmentList.FirstOrDefault(s => s.id == shipment.id);
            Assert.IsNotNull(savedShipment);
            Assert.AreEqual("FedEx", savedShipment?.carrier_code);
            Assert.AreEqual(3, savedShipment?.total_package_count);
        }

        // Test: Get Shipment by ID
        [TestMethod]
        public void Test_Get_Shipment()
        {
            var shipmentId = 1;

            var shipmentList = ReadShipmentsFromJson();
            var fetchedShipment = shipmentList.FirstOrDefault(s => s.id == shipmentId);

            Assert.IsNotNull(fetchedShipment);
            Assert.AreEqual("DPD", fetchedShipment?.carrier_code);
            Assert.AreEqual("Pending", fetchedShipment?.shipment_status);
        }

        // Test: Update Shipment
        [TestMethod]
        public void Test_Update_Shipment()
        {
            var shipmentId = 1;
            var updatedShipmentStatus = "Delivered";

            var shipmentList = ReadShipmentsFromJson();
            var shipmentToUpdate = shipmentList.FirstOrDefault(s => s.id == shipmentId);
            if (shipmentToUpdate != null)
            {
                shipmentToUpdate.shipment_status = updatedShipmentStatus;
                shipmentToUpdate.updated_at = DateTime.Now;
                WriteShipmentsToJson(shipmentList);
            }

            var updatedShipment = shipmentList.FirstOrDefault(s => s.id == shipmentId);
            Assert.IsNotNull(updatedShipment);
            Assert.AreEqual(updatedShipmentStatus, updatedShipment?.shipment_status);
        }

        // Test: Delete Shipment
        [TestMethod]
        public void Test_Delete_Shipment()
        {
            var shipment = new ShipmentModel
            {
                id = 3,
                order_id = 1003,
                source_id = 3,
                order_date = "2024-11-10",
                request_date = "2024-11-11",
                shipment_date = "2024-11-12",
                shipment_type = "Standard",
                shipment_status = "Shipped",
                notes = "Handle with care",
                carrier_code = "DHL",
                carrier_description = "DHL Express",
                service_code = "DHL_Express",
                payment_type = "Prepaid",
                transfer_mode = "Sea",
                total_package_count = 4,
                total_package_weight = 10.2f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { item_id = "P012345", amount = 2 }
                }
            };

            var shipmentList = ReadShipmentsFromJson();
            shipmentList.Add(shipment);
            WriteShipmentsToJson(shipmentList);

            shipmentList.Remove(shipment);
            WriteShipmentsToJson(shipmentList);

            var deletedShipment = shipmentList.FirstOrDefault(s => s.id == shipment.id);
            Assert.IsNull(deletedShipment);
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
