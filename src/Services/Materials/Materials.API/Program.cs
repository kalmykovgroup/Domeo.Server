using Auth.Contracts;
using Domeo.Shared.Infrastructure;
using Domeo.Shared.Infrastructure.Logging;
using Domeo.Shared.Infrastructure.Middleware;
using Materials.API.Infrastructure;
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

    Log.Information("Starting Materials.API in {Environment} mode", builder.Environment.EnvironmentName);

    // Serilog with Redis sink for Error/Fatal logs
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
        .WriteTo.Redis(services, serviceName: "Materials.API"));

    // Auth & Minimal Infrastructure (no database)
    builder.Services.AddAuthContracts(builder.Configuration);
    builder.Services.AddMinimalInfrastructure(builder.Configuration);

    // Application
    builder.Services.AddApplication();

    // External Supplier API Client
    builder.Services.AddSupplierApiClient(builder.Configuration);

    // Controllers
    builder.Services.AddControllers();

    // Health checks
    builder.Services.AddHealthChecks();

    // OpenAPI
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Global Exception Handler (first in pipeline)
    app.UseGlobalExceptionHandler();

    // Development
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

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
