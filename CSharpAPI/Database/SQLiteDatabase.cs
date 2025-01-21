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
        public DbSet<OrderShipmentMapping> OrderShipments { get; set; }
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
        public DbSet<History> History { get; set; }
        public DbSet<Contact> contacts { get; set; }
        public DbSet<ApiUser> ApiUsers { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<DockModel> DockModels { get; set; }
        public DbSet<ArchivedOrderModel> ArchivedOrders { get; set; }
        public DbSet<ArchivedShipmentModel> ArchivedShipments { get; set; }
        public DbSet<ArchivedItemModel> ArchivedItems { get; set; }
        public DbSet<ArchivedClientModel> ArchivedClients { get; set; }
        public DbSet<ArchivedDockModel> ArchivedDocks { get; set; }

        public DbSet<ArchivedInventorieModel> ArchivedInventories { get; set; }
        public DbSet<ArchivedItemGroupModel> ArchivedItemGroups { get; set; }
        public DbSet<ArchivedItemLineModel> ArchivedItemLines { get; set; }
        public DbSet<ArchivedItemTypeModel> ArchivedItemTypes { get; set; }
        public DbSet<ArchivedLocationModel> ArchivedLocations { get; set; }
        public DbSet<ArchivedSupplierModel> ArchivedSuppliers { get; set; }
        public DbSet<ArchivedTransferModel> ArchivedTransfers { get; set; }
        public DbSet<ArchivedWarehouseModel> ArchivedWarehouses { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) optionsBuilder.UseSqlite("Data Source=./Database/Data.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WarehouseModel>().OwnsOne(w => w.contact);
            modelBuilder.Entity<ClientModel>().OwnsOne(c => c.contact);
            modelBuilder.Entity<SupplierModel>().OwnsOne(c => c.contact);

            #pragma warning disable
            modelBuilder.Entity<ShipmentModel>().Property(x => x.items)
                .HasConversion(new ValueConverter<List<Items>, string>(
                    i => JsonConvert.SerializeObject(i),
                    i => JsonConvert.DeserializeObject<List<Items>>(i)));

            modelBuilder.Entity<OrderModel>().Property(x => x.items)
                .HasConversion(new ValueConverter<List<Items>, string>(
                    i => JsonConvert.SerializeObject(i),
                    i => JsonConvert.DeserializeObject<List<Items>>(i)));



            modelBuilder.Entity<OrderShipmentMapping>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
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