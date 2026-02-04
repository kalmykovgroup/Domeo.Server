using Domeo.Shared.Auth;
using Domeo.Shared.Infrastructure;
using Domeo.Shared.Infrastructure.Middleware;
using Domeo.Shared.Infrastructure.Resilience;
using Microsoft.EntityFrameworkCore;
using Modules.API.Endpoints;
using Modules.API.Persistence;
using Modules.API.Services;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Modules.API");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

    // Database
    builder.Services.AddDbContext<ModulesDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("ModulesDb"),
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "modules")));

    // Auth & Infrastructure with resilience
    builder.Services.AddSharedAuth(builder.Configuration);
    builder.Services.AddResilientInfrastructure<ModulesDbContext>(builder.Configuration, "Modules.API");

    // Services
    builder.Services.AddScoped<ModulesSeeder>();

    // OpenAPI
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Global Exception Handler (first in pipeline)
    app.UseGlobalExceptionHandler();

    // Database availability middleware (returns 503 if DB unavailable)
    app.UseDatabaseAvailability();

    // Development
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    // Database initialization - resilient startup (never throws)
    await InitializeDatabaseAsync(app);

    // Middleware Pipeline
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCurrentUser();

    // Endpoints
    app.MapHealthChecks("/health");
    app.MapModulesEndpoints();

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

async Task InitializeDatabaseAsync(WebApplication app)
{
    var stateTracker = app.Services.GetRequiredService<IConnectionStateTracker>();

    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ModulesDbContext>();

        // Check if database is available (with timeout)
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        bool canConnect;
        try
        {
            canConnect = await db.Database.CanConnectAsync(cts.Token);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database connection check failed. Starting in degraded mode");
            stateTracker.SetDatabaseAvailable(false);
            return;
        }

        if (!canConnect)
        {
            Log.Warning("Database unavailable at startup. Starting in degraded mode");
            stateTracker.SetDatabaseAvailable(false);
            return;
        }

        if (app.Environment.IsDevelopment())
        {
            Log.Information("Development mode: Recreating database...");
            await db.Database.EnsureDeletedAsync(cts.Token);
            await db.Database.EnsureCreatedAsync(cts.Token);
            Log.Information("Database recreated successfully");

            var seeder = scope.ServiceProvider.GetRequiredService<ModulesSeeder>();
            await seeder.SeedAsync();
            Log.Information("Database seeding completed");
        }
        else
        {
            await db.Database.MigrateAsync(cts.Token);
            Log.Information("Database migrations applied successfully");
        }

        stateTracker.SetDatabaseAvailable(true);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database initialization failed. Starting in degraded mode");
        stateTracker.SetDatabaseAvailable(false);
    }
}
