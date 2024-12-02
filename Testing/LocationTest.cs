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
    public class LocationIntegrationTest
    {
        private string _jsonFilePath = "data/location.json";

        private List<LocationModel> ReadLocationsFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
            var json = File.ReadAllText(_jsonFilePath);
            return JsonConvert.DeserializeObject<List<LocationModel>>(json) ?? new List<LocationModel>();
        }

        private void WriteLocationsToJson(List<LocationModel> locations)
        {
            var json = JsonConvert.SerializeObject(locations, Formatting.Indented);
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

            // Initialize location data for testing with the provided real JSON data
            var locationList = new List<LocationModel>
            {
                new LocationModel
                {
                    id = 1,
                    warehouse_id = 1,
                    code = "A.1.0",
                    name = "Row: A, Rack: 1, Shelf: 0",
                    created_at = DateTime.Parse("1992-05-15 03:21:32"),
                    updated_at = DateTime.Parse("1992-05-15 03:21:32")
                }
            };

            WriteLocationsToJson(locationList);
        }

        // Test: Add Location
        [TestMethod]
        public void Test_Add_Location()
        {
            var location = new LocationModel
            {
                id = 2,
                warehouse_id = 2,
                code = "B.2.1",
                name = "Row: B, Rack: 2, Shelf: 1",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var locationList = ReadLocationsFromJson();
            locationList.Add(location);
            WriteLocationsToJson(locationList);

            var savedLocation = locationList.FirstOrDefault(l => l.id == location.id);
            Assert.IsNotNull(savedLocation);
            Assert.AreEqual("B.2.1", savedLocation?.code);
            Assert.AreEqual("Row: B, Rack: 2, Shelf: 1", savedLocation?.name);
        }

        // Test: Get Location by ID
        [TestMethod]
        public void Test_Get_Location()
        {
            var locationId = 1;

            var locationList = ReadLocationsFromJson();
            var fetchedLocation = locationList.FirstOrDefault(l => l.id == locationId);

            Assert.IsNotNull(fetchedLocation);
            Assert.AreEqual("A.1.0", fetchedLocation?.code);
            Assert.AreEqual("Row: A, Rack: 1, Shelf: 0", fetchedLocation?.name);
        }

        // Test: Update Location
        [TestMethod]
        public void Test_Update_Location()
        {
            var locationId = 1;
            var updatedName = "Updated Row: A, Rack: 1, Shelf: 0";

            var locationList = ReadLocationsFromJson();
            var locationToUpdate = locationList.FirstOrDefault(l => l.id == locationId);
            if (locationToUpdate != null)
            {
                locationToUpdate.name = updatedName;
                locationToUpdate.updated_at = DateTime.Now;
                WriteLocationsToJson(locationList);
            }

            var updatedLocation = locationList.FirstOrDefault(l => l.id == locationId);
            Assert.IsNotNull(updatedLocation);
            Assert.AreEqual(updatedName, updatedLocation?.name);
        }

        // Test: Delete Location
        [TestMethod]
        public void Test_Delete_Location()
        {
            var location = new LocationModel
            {
                id = 3,
                warehouse_id = 3,
                code = "C.3.2",
                name = "Row: C, Rack: 3, Shelf: 2",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var locationList = ReadLocationsFromJson();
            locationList.Add(location);
            WriteLocationsToJson(locationList);

            locationList.Remove(location);
            WriteLocationsToJson(locationList);

            var deletedLocation = locationList.FirstOrDefault(l => l.id == location.id);
            Assert.IsNull(deletedLocation);
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
