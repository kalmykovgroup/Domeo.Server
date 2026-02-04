using MockSupplier.API.Endpoints;
using MockSupplier.API.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DataStore>();

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

app.UseCors();

app.MapSupplierEndpoints();

Console.WriteLine("===========================================");
Console.WriteLine("  Mock Supplier Service");
Console.WriteLine("  http://localhost:5102");
Console.WriteLine("===========================================");
Console.WriteLine();
Console.WriteLine("Endpoints:");
Console.WriteLine("  GET /api/categories          - Категории материалов");
Console.WriteLine("  GET /api/materials           - Список материалов");
Console.WriteLine("  GET /api/suppliers           - Список поставщиков");
Console.WriteLine("  GET /api/offers?materialId=  - Предложения по материалу");
Console.WriteLine();

app.Run();
