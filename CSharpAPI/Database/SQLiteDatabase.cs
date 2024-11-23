using CSharpAPI.Models;using Microsoft.Data.Sqlite;

namespace CSharpAPI.Data 
{
    public class SQLiteDatabase
    {
        public readonly string dbPath = "./Database/Data.db";

        public void SetupDatabase()
        {
            // Zorg dat de Database directory bestaat
            var databaseDir = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(databaseDir))
            {
                Directory.CreateDirectory(databaseDir!);
            }

            // SQLite maakt automatisch de database aan als deze niet bestaat
            CreateTables();
            Console.WriteLine("Database has been created/verified successfully!");
        }

        private void CreateTables()
        {
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                ClientTable ClientTable = new ClientTable();
                InventoriesTable inventoriesTable = new InventoriesTable();
                ItemTable itemTable = new ItemTable();
                XModelTable XModelTable = new XModelTable();
                LocationTable locationTable = new LocationTable();
                OrderTable orderTable = new OrderTable();
                ShipmentTable shipmentTable = new ShipmentTable();
                SuppliersTable suppliersTable = new SuppliersTable();
                TransferTable transferTable = new TransferTable();
                WarehouseTable warehouseTable = new WarehouseTable();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = ClientTable.clientQuery;
                    command.ExecuteNonQuery();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = inventoriesTable.inventorieQuery;
                    command.ExecuteNonQuery();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = itemTable.itemsQuery;
                    command.ExecuteNonQuery();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = suppliersTable.supplierQuery;
                    command.ExecuteNonQuery();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = locationTable.locationQuery;
                    command.ExecuteNonQuery();
                }

                foreach (var table in XModelTable.xModelTableQuery)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = table.Value;
                    command.ExecuteNonQuery();
                }

                foreach (var table in orderTable.orderQuery)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = table.Value;
                    command.ExecuteNonQuery();
                }

                foreach (var table in shipmentTable.shipmentQuery)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = table.Value;
                    command.ExecuteNonQuery();
                }

                foreach (var table in transferTable.transferQuery)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = table.Value;
                    command.ExecuteNonQuery();
                }

                foreach (var table in warehouseTable.warhouseQuery)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = table.Value;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
