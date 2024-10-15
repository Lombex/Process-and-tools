var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Urls.Add("http://localhost:5001");
app.MapGet("/", () => "Hello World!");

app.Run();
