using CSharpAPI.Models;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace CSharpAPI.Data
{
    public class SQLiteDatabase
    {
        public readonly string dbPath = "./Database/Data.db";
        public void SetupDatabase()
        {
            if (!System.IO.File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                Console.WriteLine("Database has been created successfully!");
            }

            CreateTables();
        }

        private void CreateTables()
        {
            using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
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

                using (var command = new SQLiteCommand(ClientTable.clientQuery, connection)) command.ExecuteNonQuery();
                
                using (var command = new SQLiteCommand(inventoriesTable.inventorieQuery, connection)) command.ExecuteNonQuery();
                
                using (var command = new SQLiteCommand(itemTable.itemsQuery, connection)) command.ExecuteNonQuery();

                using (var command = new SQLiteCommand(suppliersTable.supplierQuery, connection)) command.ExecuteNonQuery();

                using (var command = new SQLiteCommand(locationTable.locationQuery, connection)) command.ExecuteNonQuery();

                foreach (var table in XModelTable.xModelTableQuery) 
                    using (var command = new SQLiteCommand(table.Value, connection)) command.ExecuteNonQuery();

                foreach (var table in orderTable.orderQuery)
                    using (var command = new SQLiteCommand(table.Value, connection)) command.ExecuteNonQuery();

                foreach (var table in shipmentTable.shipmentQuery)
                    using (var command = new SQLiteCommand(table.Value, connection)) command.ExecuteNonQuery();
   
                foreach (var table in transferTable.transferQuery)
                   using (var command = new SQLiteCommand(table.Value, connection)) command.ExecuteNonQuery();

                foreach (var table in warehouseTable.warhouseQuery) 
                    using (var command = new SQLiteCommand(table.Value, connection)) command.ExecuteNonQuery();
            }
        }
    }
}