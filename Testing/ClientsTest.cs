// Converted Tests for CSharpAPI
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Tests
{
    [TestClass]
    public class ClientIntegrationTest
    {
        private string DatabasePath => Path.Combine(AppContext.BaseDirectory, "Database", "Data.db");

        [TestInitialize]
        public void SetupDatabase()
        {
            var directory = Path.GetDirectoryName(DatabasePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory); // Zorg dat de map bestaat
            }

            var options = new DbContextOptionsBuilder<CSharpAPI.Data.SQLiteDatabase>()
                .UseSqlite($"Data Source={DatabasePath}")
                .Options;

            using (var db = new CSharpAPI.Data.SQLiteDatabase(options))
            {
                db.Database.EnsureDeleted(); // Reset de database
                db.Database.EnsureCreated(); // Controleer dat de database en tabellen worden aangemaakt

                // Voeg seed data toe
                db.ClientModels.Add(new CSharpAPI.Models.ClientModel
                {
                    id = 1,
                    name = "Raymond Inc",
                    address = "1296 Daniel Road Apt. 349",
                    city = "Pierceview",
                    zip_code = "28301",
                    province = "Colorado",
                    country = "United States",
                    contact_name = "Bryan Clark",
                    contact_phone = "242.732.3483x2573",
                    contact_email = "bryan.clark@example.com",
                    created_at = DateTime.Parse("2010-04-28 02:22:53"),
                    updated_at = DateTime.Parse("2022-02-09 20:22:35")
                });

                db.SaveChanges();
            }
        }

        [TestMethod]
        public void Test_Add_Client_Database()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabasePath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO ClientModels (id, name, address, city, zip_code, province, country, contact_name, contact_phone, contact_email, created_at, updated_at)
                    VALUES (2, 'Tech Solutions', '789 Tech Park', 'Silicon Valley', '94016', 'California', 'United States', 'Jane Smith', '555-987-6543', 'janesmith@techsolutions.com', @CreatedAt, @UpdatedAt)";
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM ClientModels WHERE id = 2";
                using (var reader = command.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Tech Solutions", reader["name"].ToString());
                    Assert.AreEqual("janesmith@techsolutions.com", reader["contact_email"].ToString());
                }
            }
        }

        [TestMethod]
        public void Test_Get_Client_Database()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabasePath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM ClientModels WHERE id = 1";
                using (var reader = command.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Raymond Inc", reader["name"].ToString());
                    Assert.AreEqual("242.732.3483x2573", reader["contact_phone"].ToString());
                }
            }
        }

        [TestMethod]
        public void Test_Update_Client_Database()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabasePath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "UPDATE ClientModels SET contact_phone = @ContactPhone WHERE id = 1";
                command.Parameters.AddWithValue("@ContactPhone", "555-000-1234");
                command.ExecuteNonQuery();

                command.CommandText = "SELECT contact_phone FROM ClientModels WHERE id = 1";
                using (var reader = command.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("555-000-1234", reader["contact_phone"].ToString());
                }
            }
        }

        [TestMethod]
        public void Test_Delete_Client_Database()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabasePath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO ClientModels (id, name, address, city, zip_code, province, country, contact_name, contact_phone, contact_email, created_at, updated_at)
                    VALUES (3, 'Gamma Enterprises', '123 Gamma St', 'Star City', '12345', 'Illinois', 'USA', 'Alice Cooper', '555-111-2222', 'alicecooper@gamma.com', @CreatedAt, @UpdatedAt)";
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                command.ExecuteNonQuery();

                command.CommandText = "DELETE FROM ClientModels WHERE id = 3";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM ClientModels WHERE id = 3";
                using (var reader = command.ExecuteReader())
                {
                    Assert.IsFalse(reader.Read());
                }
            }
        }
    }
}
