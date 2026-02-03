using Auth.API.Endpoints;
using Auth.API.Persistence;
using Auth.API.Services;
using Domeo.Shared.Auth;
using Domeo.Shared.Infrastructure;
using Domeo.Shared.Infrastructure.Middleware;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Auth.API");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

    // Database
    builder.Services.AddDbContext<AuthDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("AuthDb"),
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "auth")));

    // Auth & Infrastructure
    builder.Services.AddSharedAuth(builder.Configuration);
    builder.Services.AddSharedInfrastructure(builder.Configuration, "Auth.API");

    // Services
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

    // HTTP Clients
    builder.Services.AddTransient<InternalApiKeyDelegatingHandler>();
    builder.Services.AddHttpClient("UsersApi", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Services:UsersApi"] ?? "http://localhost:5002");
    }).AddHttpMessageHandler<InternalApiKeyDelegatingHandler>();

    // OpenAPI
    builder.Services.AddOpenApi();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("AuthDb")!);

    var app = builder.Build();

    // Global Exception Handler (first in pipeline)
    app.UseGlobalExceptionHandler();

    // Development
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    // Database initialization
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        if (app.Environment.IsDevelopment())
        {
            Log.Information("Development mode: Recreating database...");
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
            Log.Information("Database recreated successfully");
        }
        else
        {
            await db.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database initialization failed. Ensure PostgreSQL is running");
        if (!app.Environment.IsDevelopment())
            throw;
    }

    // Middleware Pipeline
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCurrentUser();

    // Endpoints
    app.MapHealthChecks("/health");
    app.MapAuthEndpoints();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
