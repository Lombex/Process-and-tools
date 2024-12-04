using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Data 
{
    public class SQLiteDatabase : DbContext
    {
        public SQLiteDatabase(DbContextOptions<SQLiteDatabase> options) : base(options) { }
        public DbSet<WarehouseModel> Warehouse { get; set; }
        public DbSet<TransferModel> Transfer { get; set; }
        public DbSet<SupplierModel> Suppliers { get; set; }
        public DbSet<ShipmentModel> Shipment { get; set; }
        public DbSet<OrderModel> Order { get; set; }
        public DbSet<LocationModel> Location { get; set; }
        public DbSet<ItemGroupModel> ItemGroups { get; set; }
        public DbSet<ItemLineModel> ItemLine { get; set; }
        public DbSet<ItemTypeModel> ItemType { get; set; }
        public DbSet<ItemModel> itemModels { get; set; }
        public DbSet<Items> Items { get; set; }
        public DbSet<InventorieModel> Inventors { get; set; }
        public DbSet<ClientModel> ClientModels { get; set; }
        public DbSet<Contact> contacts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) optionsBuilder.UseSqlite("Data Source=./Database/Data.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}