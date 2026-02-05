using Clients.API.Infrastructure;
using Clients.API.Infrastructure.Persistence;
using Auth.Contracts;
using Domeo.Shared.Infrastructure;
using Domeo.Shared.Infrastructure.Logging;
using Domeo.Shared.Infrastructure.Middleware;
using Domeo.Shared.Infrastructure.Resilience;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    Log.Information("Starting Clients.API in {Environment} mode", builder.Environment.EnvironmentName);

    // Serilog with Redis sink for Error/Fatal logs
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
        .WriteTo.Redis(services, serviceName: "Clients.API"));

    // Database
    builder.Services.AddDbContext<ClientsDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "clients")));

    // Auth & Infrastructure with resilience
    builder.Services.AddAuthContracts(builder.Configuration);
    builder.Services.AddResilientInfrastructure<ClientsDbContext>(builder.Configuration, "Clients.API");

    // Application & Infrastructure (MediatR, Repositories)
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure();

    // Controllers
    builder.Services.AddControllers();

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

    // Endpoints
    app.MapHealthChecks("/health");
    app.MapControllers();

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
        var db = scope.ServiceProvider.GetRequiredService<ClientsDbContext>();

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

        await db.Database.MigrateAsync(cts.Token);
        Log.Information("Database migrations applied successfully");

        stateTracker.SetDatabaseAvailable(true);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database initialization failed. Starting in degraded mode");
        stateTracker.SetDatabaseAvailable(false);
    }
}
