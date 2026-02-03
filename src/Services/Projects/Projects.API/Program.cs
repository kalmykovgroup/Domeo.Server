using Domeo.Shared.Auth;
using Domeo.Shared.Infrastructure;
using Domeo.Shared.Infrastructure.Middleware;
using Microsoft.EntityFrameworkCore;
using Projects.API.Endpoints;
using Projects.API.Persistence;
using Projects.API.Services;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Projects.API");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

    // Database
    builder.Services.AddDbContext<ProjectsDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("ProjectsDb"),
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "projects")));

    // Auth & Infrastructure
    builder.Services.AddSharedAuth(builder.Configuration);
    builder.Services.AddSharedInfrastructure(builder.Configuration, "Projects.API");

    // Services
    builder.Services.AddScoped<ProjectsSeeder>();

    // OpenAPI
    builder.Services.AddOpenApi();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("ProjectsDb")!);

    var app = builder.Build();

    // Global Exception Handler (first in pipeline)
    app.UseGlobalExceptionHandler();

    // Development
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    // Database initialization and seeding
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();

        if (app.Environment.IsDevelopment())
        {
            Log.Information("Development mode: Recreating database...");
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
            Log.Information("Database recreated successfully");

            var seeder = scope.ServiceProvider.GetRequiredService<ProjectsSeeder>();
            await seeder.SeedAsync();
            Log.Information("Database seeding completed");
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
    app.MapProjectsEndpoints();

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
