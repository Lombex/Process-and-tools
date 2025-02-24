using CSharpAPI.Service;
using CSharpAPI.Services;
using CSharpAPI.Middleware;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using CSharpAPI.Services.Auth;

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
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<ITransfersService, TransferSerivce>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IItemsService, ItemsService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IInventoriesService, InventoriesService>();
builder.Services.AddScoped<IClientsService, ClientsService>();
builder.Services.AddScoped<IItemTypeService, ItemTypeService>();
builder.Services.AddScoped<IItemLineService, ItemLineService>();
builder.Services.AddScoped<IItemGroupService, ItemGroupService>();
builder.Services.AddScoped<IDockService, DockService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<HistoryService>();

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

// Add Swagger with API Key support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CSharp API", Version = "v1" });
    
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key must appear in header",
        Type = SecuritySchemeType.ApiKey,
        Name = "API_KEY",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });
    
    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        },
        In = ParameterLocation.Header
    };
    
    var requirement = new OpenApiSecurityRequirement
    {
        { scheme, new List<string>() }
    };
    
    c.AddSecurityRequirement(requirement);
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

// Middleware in correct order
app.UseRouting();
app.UseCors("AllowAll");
app.UseMiddleware<AuthMiddleware>();
app.UseAuthorization();
app.MapControllers();

// Set URL
app.Urls.Add("http://localhost:5001");

// Configure Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SQLiteDatabase>();
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await dbContext.Database.MigrateAsync();
        await DatabaseSeeding.SeedDatabase(dbContext, authService);
        
        // Ensure permissions for all existing API keys
        var existingUsers = await dbContext.ApiUsers.ToListAsync();
        foreach (var user in existingUsers)
        {
            await authService.EnsureRolePermissions(user.role);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

app.Run();