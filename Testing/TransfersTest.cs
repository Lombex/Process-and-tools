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
    public class TransferIntegrationTest
    {
        private string _jsonFilePath = "data/transfer.json";

        private List<TransferModel> ReadTransfersFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
            var json = File.ReadAllText(_jsonFilePath);

            var transferList = JsonConvert.DeserializeObject<List<TransferModel>>(json);

            return transferList ?? new List<TransferModel>();
        }

        private void WriteTransfersToJson(List<TransferModel> transfers)
        {
            var json = JsonConvert.SerializeObject(transfers, Formatting.Indented);
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

            // Initialize transfer data for testing
            var transferList = new List<TransferModel>
            {
                new TransferModel
                {
                    id = 1,
                    reference = "TR00001",
                    transfer_from = null,
                    transfer_to = 9229,
                    transfer_status = "Completed",
                    created_at = DateTime.Parse("2000-03-11T13:11:14Z"),
                    updated_at = DateTime.Parse("2000-03-12T16:11:14Z"),
                    items = new List<Items>
                    {
                        new Items { item_id = "P007435", amount = 23 }
                    }
                }
            };

            WriteTransfersToJson(transferList);
        }

        // Test: Add Transfer
        [TestMethod]
        public void Test_Add_Transfer()
        {
            var transfer = new TransferModel
            {
                id = 2,
                reference = "TR00002",
                transfer_from = 1000,
                transfer_to = 2000,
                transfer_status = "Pending",
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { item_id = "P000123", amount = 50 }
                }
            };

            var transferList = ReadTransfersFromJson();
            transferList.Add(transfer);
            WriteTransfersToJson(transferList);

            var savedTransfer = transferList.FirstOrDefault(t => t.id == transfer.id);
            Assert.IsNotNull(savedTransfer);
            Assert.AreEqual("TR00002", savedTransfer?.reference);
            Assert.AreEqual(50, savedTransfer?.items.First().amount);
        }

        // Test: Get Transfer by ID
        [TestMethod]
        public void Test_Get_Transfer()
        {
            var transferId = 1;

            var transferList = ReadTransfersFromJson();
            var fetchedTransfer = transferList.FirstOrDefault(t => t.id == transferId);

            Assert.IsNotNull(fetchedTransfer);
            Assert.AreEqual("TR00001", fetchedTransfer?.reference);
            Assert.AreEqual("Completed", fetchedTransfer?.transfer_status);
            Assert.AreEqual(23, fetchedTransfer?.items.First().amount);
        }

        // Test: Delete Transfer
        [TestMethod]
        public void Test_Delete_Transfer()
        {
            var transfer = new TransferModel
            {
                id = 3,
                reference = "TR00003",
                transfer_from = 2000,
                transfer_to = 3000,
                transfer_status = "In Progress",
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new List<Items>
                {
                    new Items { item_id = "P009876", amount = 100 }
                }
            };
            var transferList = ReadTransfersFromJson();
            transferList.Add(transfer);
            WriteTransfersToJson(transferList);

            transferList.Remove(transfer);
            WriteTransfersToJson(transferList);

            var deletedTransfer = transferList.FirstOrDefault(t => t.id == transfer.id);
            Assert.IsNull(deletedTransfer);
        }

        // Test: Update Transfer
        [TestMethod]
        public void Test_Update_Transfer()
        {
            var transferId = 1;
            var updatedStatus = "Cancelled";

            var transferList = ReadTransfersFromJson();
            var transferToUpdate = transferList.FirstOrDefault(t => t.id == transferId); 
            if (transferToUpdate != null)
            {
                transferToUpdate.transfer_status = updatedStatus;
                transferToUpdate.updated_at = DateTime.Now;
                WriteTransfersToJson(transferList);
            }

            var updatedTransfer = transferList.FirstOrDefault(t => t.id == transferId); 
            Assert.IsNotNull(updatedTransfer);
            Assert.AreEqual(updatedStatus, updatedTransfer?.transfer_status);
        }

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
