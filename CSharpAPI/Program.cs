
using CSharpAPI.Data;
using CSharpAPI.Service;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddSingleton<IWarehouseService, WarehouseService>();

var app = builder.Build();

SQLiteDatabase sQLiteDatabase = new SQLiteDatabase();
sQLiteDatabase.SetupDatabase();

app.MapControllers();
app.MapGet("/", () => "Hello World!");
app.Urls.Add("http://localhost:5001"); 

app.Run();

