using MockAuthCenter.API.Services;
using MockAuthCenter.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Starting MockAuthCenter.API in {builder.Environment.EnvironmentName} mode");

// Services
builder.Services.AddSingleton<RsaKeyService>();
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<AuthCodeStore>();
builder.Services.AddSingleton<RefreshTokenStore>();
builder.Services.AddSingleton<TokenService>();

// CORS for development
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

// Map endpoints
app.MapAuthCenterEndpoints();

Console.WriteLine("Mock Auth Center started on http://localhost:5100");
Console.WriteLine("Test users:");
Console.WriteLine("  admin@test.com / admin123 (role: admin)");
Console.WriteLine("  manager@test.com / manager123 (role: manager)");
Console.WriteLine("  viewer@test.com / viewer123 (role: viewer)");

app.Run();
