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
    public class OrderIntegrationTest
    {
        private string _jsonFilePath = "data/order.json";

        private List<OrderModel> ReadOrdersFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
            var json = File.ReadAllText(_jsonFilePath);
            return JsonConvert.DeserializeObject<List<OrderModel>>(json) ?? new List<OrderModel>();
        }

        private void WriteOrdersToJson(List<OrderModel> orders)
        {
            var json = JsonConvert.SerializeObject(orders, Formatting.Indented);
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

            // Initialize order data for testing with the provided real JSON data
            var orderList = new List<OrderModel>
            {
                new OrderModel
                {
                    id = 1,
                    source_id = 33,
                    order_date = "2019-04-03T11:33:15Z",
                    request_date = "2019-04-07T11:33:15Z",
                    reference = "ORD00001",
                    reference_extra = "Bedreven arm straffen bureau.",
                    order_status = "Delivered",
                    notes = "Voedsel vijf vork heel.",
                    shipping_notes = "Buurman betalen plaats bewolkt.",
                    picking_notes = "Ademen fijn volgorde scherp aardappel op leren.",
                    warehouse_id = 18,
                    ship_to = 5,
                    bill_to = 6,
                    shipment_id = 1,
                    total_amount = 9905.13f,
                    total_discount = 150.77f,
                    total_tax = 372.72f,
                    total_surcharge = 77.6f,
                    created_at = DateTime.Parse("2019-04-03T11:33:15Z"),
                    updated_at = DateTime.Parse("2019-04-05T07:33:15Z"),
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

            WriteOrdersToJson(orderList);
        }

        // Test: Add Order
        [TestMethod]
        public void Test_Add_Order()
        {
            var order = new OrderModel
            {
                id = 2,
                source_id = 34,
                order_date = "2024-11-10T10:00:00Z",
                request_date = "2024-11-12T12:00:00Z",
                reference = "ORD10002",
                reference_extra = "Extra details here.",
                order_status = "Pending",
                notes = "New order, urgent delivery.",
                shipping_notes = "Handle with care.",
                picking_notes = "Pick items from A2.",
                warehouse_id = 19,
                ship_to = 1001,
                bill_to = 1002,
                shipment_id = 2,
                total_amount = 1200.75f,
                total_discount = 50.5f,
                total_tax = 100.0f,
                total_surcharge = 10.0f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { item_id = "P012345", amount = 5 }
                }
            };

            var orderList = ReadOrdersFromJson();
            orderList.Add(order);
            WriteOrdersToJson(orderList);

            var savedOrder = orderList.FirstOrDefault(o => o.id == order.id);
            Assert.IsNotNull(savedOrder);
            Assert.AreEqual("ORD10002", savedOrder?.reference);
            Assert.AreEqual(1, savedOrder?.items.Count);
        }

        // Test: Get Order by ID
        [TestMethod]
        public void Test_Get_Order()
        {
            var orderId = 1;

            var orderList = ReadOrdersFromJson();
            var fetchedOrder = orderList.FirstOrDefault(o => o.id == orderId);

            Assert.IsNotNull(fetchedOrder);
            Assert.AreEqual("ORD00001", fetchedOrder?.reference);
            Assert.AreEqual("Delivered", fetchedOrder?.order_status);
        }

        // Test: Update Order
        [TestMethod]
        public void Test_Update_Order()
        {
            var orderId = 1;
            var updatedOrderStatus = "Shipped";

            var orderList = ReadOrdersFromJson();
            var orderToUpdate = orderList.FirstOrDefault(o => o.id == orderId);
            if (orderToUpdate != null)
            {
                orderToUpdate.order_status = updatedOrderStatus;
                orderToUpdate.updated_at = DateTime.Now;
                WriteOrdersToJson(orderList);
            }

            var updatedOrder = orderList.FirstOrDefault(o => o.id == orderId);
            Assert.IsNotNull(updatedOrder);
            Assert.AreEqual(updatedOrderStatus, updatedOrder?.order_status);
        }

        // Test: Delete Order
        [TestMethod]
        public void Test_Delete_Order()
        {
            var order = new OrderModel
            {
                id = 3,
                source_id = 35,
                order_date = "2024-11-15T14:00:00Z",
                request_date = "2024-11-17T15:00:00Z",
                reference = "ORD10003",
                reference_extra = "Additional details.",
                order_status = "Canceled",
                notes = "Customer canceled the order.",
                shipping_notes = "N/A",
                picking_notes = "N/A",
                warehouse_id = 20,
                ship_to = 2001,
                bill_to = 2002,
                shipment_id = 3,
                total_amount = 800.50f,
                total_discount = 30.0f,
                total_tax = 60.0f,
                total_surcharge = 5.0f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { item_id = "P010101", amount = 1 }
                }
            };

            var orderList = ReadOrdersFromJson();
            orderList.Add(order);
            WriteOrdersToJson(orderList);

            orderList.Remove(order);
            WriteOrdersToJson(orderList);

            var deletedOrder = orderList.FirstOrDefault(o => o.id == order.id);
            Assert.IsNull(deletedOrder);
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
