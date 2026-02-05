using Microsoft.EntityFrameworkCore;
using MockSupplier.API.Endpoints;
using MockSupplier.API.Infrastructure.Persistence;
using MockSupplier.API.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Starting MockSupplier.API in {builder.Environment.EnvironmentName} mode");

// SQLite
var dbPath = Path.Combine(AppContext.BaseDirectory, "supplier.db");
builder.Services.AddDbContext<SupplierDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<SupplierDbSeeder>();

// DataStore — singleton, загружает данные из SQLite один раз
builder.Services.AddSingleton<DataStore>(sp =>
{
    using var scope = sp.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();
    return new DataStore(db);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Ensure DB created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();
    db.Database.EnsureCreated();
    var seeder = scope.ServiceProvider.GetRequiredService<SupplierDbSeeder>();
    await seeder.SeedAsync();
}

app.UseCors();

app.MapSupplierEndpoints();

Console.WriteLine("===========================================");
Console.WriteLine("  Mock Supplier Service");
Console.WriteLine("  http://localhost:5102");
Console.WriteLine($"  SQLite: {dbPath}");
Console.WriteLine("===========================================");
Console.WriteLine();
Console.WriteLine("Endpoints:");
Console.WriteLine("  GET /api/categories          - Категории материалов");
Console.WriteLine("  GET /api/materials           - Список материалов");
Console.WriteLine("  GET /api/suppliers           - Список поставщиков");
Console.WriteLine("  GET /api/offers?materialId=  - Предложения по материалу");
Console.WriteLine();

app.Run();
