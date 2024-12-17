using CSharpAPI.Data;
using CSharpAPI.Models;
using CSharpAPI.Service;
using CSharpAPI.Controller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration_Testing
{
    public class SupplierControllerTests
    {
        private SQLiteDatabase GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SQLiteDatabase(options);

            // Seed initial data for suppliers
            context.Suppliers.AddRange(
                new SupplierModel
                {
                    id = 1,
                    code = "SUP001",
                    name = "Supplier One",
                    address = "123 Test St",
                    city = "Testville",
                    zip_code = "12345",
                    province = "Test Province",
                    contact_name = "John Doe",
                    phonenumber = "123456789",
                    reference = "REF001",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new SupplierModel
                {
                    id = 2,
                    code = "SUP002",
                    name = "Supplier Two",
                    address = "456 Another St",
                    city = "Anotherville",
                    zip_code = "67890",
                    province = "Another Province",
                    contact_name = "Jane Smith",
                    phonenumber = "987654321",
                    reference = "REF002",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                }
            );

            // Seed initial data for items
            context.itemModels.AddRange(
                new ItemModel
                {
                    uid = "ITEM001",
                    code = "ITM001",
                    description = "Description for Item 1",
                    short_description = "Short Desc 1",
                    supplier_id = 1,
                    item_line = 1,
                    item_group = 1,
                    item_type = 1,
                    unit_purchase_quantity = 10,
                    unit_order_quantity = 5,
                    pack_order_quantity = 1,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new ItemModel
                {
                    uid = "ITEM002",
                    code = "ITM002",
                    description = "Description for Item 2",
                    short_description = "Short Desc 2",
                    supplier_id = 1,
                    item_line = 1,
                    item_group = 2,
                    item_type = 1,
                    unit_purchase_quantity = 20,
                    unit_order_quantity = 10,
                    pack_order_quantity = 2,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                },
                new ItemModel
                {
                    uid = "ITEM003",
                    code = "ITM003",
                    description = "Description for Item 3",
                    short_description = "Short Desc 3",
                    supplier_id = 2,
                    item_line = 2,
                    item_group = 3,
                    item_type = 2,
                    unit_purchase_quantity = 15,
                    unit_order_quantity = 7,
                    pack_order_quantity = 3,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAllSuppliers_ReturnsAllSuppliers()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new SupplierService(dbContext);
            var controller = new SupplierController(service);

            var result = await controller.GetAllSuppliers() as OkObjectResult;
            var suppliers = result?.Value as List<SupplierModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, suppliers.Count);
        }

        [Fact]
        public async Task GetSupplierById_ReturnsSupplier()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new SupplierService(dbContext);
            var controller = new SupplierController(service);

            var result = await controller.GetSupplierById(1) as OkObjectResult;
            var supplier = result?.Value as SupplierModel;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(supplier);
            Xunit.Assert.Equal("Supplier One", supplier.name);
        }

        [Fact]
        public async Task GetItemFromSupplierId_ReturnsItems()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new SupplierService(dbContext);
            var controller = new SupplierController(service);

            var result = await controller.GetItemFromSupplierId(1) as OkObjectResult;
            var items = result?.Value as List<ItemModel>;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task CreateSupplier_AddsNewSupplier()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new SupplierService(dbContext);
            var controller = new SupplierController(service);

            var newSupplier = new SupplierModel
            {
                id = 3,
                code = "SUP003",
                name = "Supplier Three",
                address = "789 New St",
                city = "Newville",
                zip_code = "11111",
                province = "New Province",
                contact_name = "New Contact",
                phonenumber = "555555555",
                reference = "REF003",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var result = await controller.CreateSupplier(newSupplier) as CreatedAtActionResult;
            var suppliers = await dbContext.Suppliers.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(201, result.StatusCode);
            Xunit.Assert.Equal(3, suppliers.Count);
            Xunit.Assert.Contains(suppliers, s => s.name == "Supplier Three");
        }

        [Fact]
        public async Task UpdateSupplier_UpdatesExistingSupplier()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new SupplierService(dbContext);
            var controller = new SupplierController(service);

            var updatedSupplier = new SupplierModel
            {
                code = "SUP001-Updated",
                name = "Supplier One Updated",
                address = "Updated Address",
                city = "Updated City",
                zip_code = "99999",
                province = "Updated Province",
                contact_name = "Updated Contact",
                phonenumber = "111111111",
                reference = "REF001-Updated",
                updated_at = DateTime.Now
            };

            var result = await controller.UpdateSupplier(1, updatedSupplier) as OkObjectResult;
            var supplier = await dbContext.Suppliers.FirstOrDefaultAsync(s => s.id == 1);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.NotNull(supplier);
            Xunit.Assert.Equal("Supplier One Updated", supplier.name);
        }

        [Fact]
        public async Task DeleteSupplier_RemovesSupplier()
        {
            var dbContext = GetInMemoryDatabaseContext();
            var service = new SupplierService(dbContext);
            var controller = new SupplierController(service);

            var result = await controller.DeleteSupplier(1) as OkObjectResult;
            var suppliers = await dbContext.Suppliers.ToListAsync();

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(200, result.StatusCode);
            Xunit.Assert.Single(suppliers);
            Xunit.Assert.DoesNotContain(suppliers, s => s.id == 1);
        }
    }
}
