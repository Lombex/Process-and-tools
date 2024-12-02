using CSharpAPI.Service;
using CSharpAPI.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add basic services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Register services
builder.Services.AddSingleton<IWarehouseService, WarehouseService>();
builder.Services.AddSingleton<IItemTypeService, ItemTypeService>();
builder.Services.AddSingleton<IItemGroupService, ItemGroupService>();
builder.Services.AddSingleton<IItemLineService, ItemLineService>();
builder.Services.AddSingleton<ITransfersService, TransferSerivce>();
builder.Services.AddSingleton<IShipmentService, ShipmentService>();
builder.Services.AddSingleton<ILocationService, LocationService>();
builder.Services.AddSingleton<IItemsService, ItemsService>();
builder.Services.AddSingleton<ISupplierService, SupplierService>();
builder.Services.AddSingleton<IInventoriesService, InventoriesService>();
builder.Services.AddSingleton<IClientsService, ClientsService>();
builder.Services.AddSingleton<IOrderService, OrderService>(); // Correct service registration
builder.Services.AddSingleton<SQLiteDatabase>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Add Authorization
builder.Services.AddAuthorization();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CSharp API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CSharp API V1");
    });
}

// Ensure the database is set up
SQLiteDatabase sQLiteDatabase = new SQLiteDatabase();
sQLiteDatabase.SetupDatabase();

// Insert warehouse data from JSON file into the database
var suppliersService = app.Services.GetRequiredService<ISupplierService>();
var orderService = app.Services.GetRequiredService<IOrderService>(); // Correct usage
orderService.InsertOrdersFromJson();  // Call the method to insert orders

suppliersService.InsertSuppliersIntoDatabase();


// Middleware in correct order
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Set URL
app.Urls.Add("http://localhost:5001");

app.Run();
