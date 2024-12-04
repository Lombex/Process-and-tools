using CSharpAPI.Service;
using CSharpAPI.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;


var builder = WebApplication.CreateBuilder(args);

// Add basic services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Add DbContext for SQLite
builder.Services.AddDbContext<SQLiteDatabase>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddSingleton<IOrderService, OrderService>();


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

// Add Authorization (nodig voor UseAuthorization)
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
// Middleware in correcte volgorde
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Set URL
app.Urls.Add("http://localhost:5001");

// Configure Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SQLiteDatabase>();
    dbContext.Database.Migrate();
}


app.Run();