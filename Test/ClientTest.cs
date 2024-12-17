using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace CSharpAPI.Tests
{
    [TestClass]
    public class ClientIntegrationTest
    {
        private string DatabasePath => Path.GetFullPath(Path.Combine(
    AppContext.BaseDirectory, 
    "../../../../CSharpAPI/Database/Data.db"));

        [TestInitialize]
        public void SetupDatabase()
        {
            // Controleer alleen of de database bestaat en toegankelijk is
            if (!File.Exists(DatabasePath))
            {
                Assert.Fail("Database bestaat niet op het opgegeven pad: " + DatabasePath);
            }
        }

        [TestMethod]
        public void Test_Get_Client_From_Database()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabasePath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM ClientModels WHERE id = 1";
                
                using (var reader = command.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read(), "Er moet een client bestaan met ID = 1.");
                    Assert.AreEqual("Raymond Inc", reader["name"].ToString(), "Naam komt niet overeen.");
                    Assert.AreEqual("555-123-4567", reader["contact_phone"].ToString(), "Telefoonnummer komt niet overeen.");
                }
            }
        }

        [TestMethod]
        public void Test_Add_New_Client()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabasePath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO ClientModels (name, address, city, zip_code, province, country, contact_name, contact_phone, contact_email, created_at, updated_at)
                    VALUES ('Test Client', '123 Test St', 'TestCity', '12345', 'TestProvince', 'TestCountry', 'Test Contact', '555-999-9999', 'test@example.com', @CreatedAt, @UpdatedAt)";
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                command.ExecuteNonQuery();

                // Verifieer dat de client is toegevoegd
                command.CommandText = "SELECT * FROM ClientModels WHERE name = 'Test Client'";
                using (var reader = command.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read(), "De nieuwe client moet in de database staan.");
                    Assert.AreEqual("Test Client", reader["name"].ToString(), "Naam komt niet overeen.");
                }
            }
        }


        [TestMethod]
        public void Test_Update_Client()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabasePath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "UPDATE ClientModels SET contact_phone = @NewPhone WHERE id = 1";
                command.Parameters.AddWithValue("@NewPhone", "555-123-4567");
                command.ExecuteNonQuery();

                // Controleer dat de update is doorgevoerd
                command.CommandText = "SELECT contact_phone FROM ClientModels WHERE id = 1";
                using (var reader = command.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read(), "Er moet een client bestaan met ID = 1.");
                    Assert.AreEqual("555-123-4567", reader["contact_phone"].ToString(), "Telefoonnummer is niet bijgewerkt.");
                }
            }
        }

        [TestMethod]
        public void Test_Delete_Client()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabasePath}"))
            {
                connection.Open();

                // Zoek het hoogste bestaande ID
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MAX(id) FROM ClientModels";
                var result = command.ExecuteScalar();
                int newId = (result == DBNull.Value ? 1 : Convert.ToInt32(result) + 1);

                // Voeg een client toe
                command.Parameters.Clear();
                command.CommandText = @"
                    INSERT INTO ClientModels (id, name, address, city, zip_code, province, country, contact_name, contact_phone, contact_email, created_at, updated_at)
                    VALUES (@Id, 'Delete Client', '123 Delete St', 'DeleteCity', '54321', 'DeleteProvince', 'DeleteCountry', 'Delete Contact', '555-000-0000', 'delete@example.com', @CreatedAt, @UpdatedAt)";
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                command.ExecuteNonQuery();

                // Verwijder de client
                command.Parameters.Clear();
                command.CommandText = "DELETE FROM ClientModels WHERE id = @Id";
                command.Parameters.AddWithValue("@Id", newId);
                command.ExecuteNonQuery();

                // Controleer dat de client is verwijderd
                command.Parameters.Clear();
                command.CommandText = "SELECT * FROM ClientModels WHERE id = @Id";
                command.Parameters.AddWithValue("@Id", newId);
                using (var reader = command.ExecuteReader())
                {
                    Assert.IsFalse(reader.Read(), "De client zou verwijderd moeten zijn.");
                }
            }
        }

    }
}
