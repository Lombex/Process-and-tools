using CSharpAPI.Models;
using CSharpAPI.Models.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

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
        public DbSet<ApiUser> ApiUsers { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) optionsBuilder.UseSqlite("Data Source=./Database/Data.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WarehouseModel>().OwnsOne(w => w.contact);

            #pragma warning disable 

            modelBuilder.Entity<TransferModel>().Property(x => x.items).HasConversion(new ValueConverter<List<Items>, string>(
                i => JsonConvert.SerializeObject(i), i => JsonConvert.DeserializeObject<List<Items>>(i)));

            modelBuilder.Entity<ShipmentModel>().Property(x => x.items).HasConversion(new ValueConverter<List<Items>, string>(
                i => JsonConvert.SerializeObject(i), i => JsonConvert.DeserializeObject<List<Items>>(i)));

            modelBuilder.Entity<OrderModel>().Property(x => x.items).HasConversion(new ValueConverter<List<Items>, string>(
                i => JsonConvert.SerializeObject(i), i => JsonConvert.DeserializeObject<List<Items>>(i)));

            // Configure ApiUser entity
            modelBuilder.Entity<ApiUser>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.api_key).IsRequired();
                entity.Property(e => e.role).IsRequired();
                entity.HasIndex(e => e.api_key).IsUnique();
            });

            // Configure RolePermission entity
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.role).IsRequired();
                entity.Property(e => e.resource).IsRequired();
                entity.HasIndex(e => new { e.role, e.resource }).IsUnique();
            });

            #pragma warning restore

            base.OnModelCreating(modelBuilder);
        }
    }
}