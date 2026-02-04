using Domeo.ApiGateway.Auth;
using Domeo.Shared.Infrastructure;
using Domeo.Shared.Infrastructure.Logging;
using Domeo.Shared.Infrastructure.Middleware;
using Domeo.ApiGateway.Gateway;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Domeo API Gateway");

    var builder = WebApplication.CreateBuilder(args);

    // Minimal infrastructure for logging to Redis
    builder.Services.AddMinimalInfrastructure(builder.Configuration);

    // Serilog with Redis sink for Error/Fatal logs
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
        .MinimumLevel.Override("Yarp", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
        .WriteTo.Redis(services, serviceName: "ApiGateway"));

    // CORS
    const string corsPolicyName = "AllowFrontend";
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

    Log.Information("CORS AllowedOrigins: {Origins}", string.Join(", ", allowedOrigins));

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(corsPolicyName, policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Required for cookies
            }
            else
            {
                Log.Warning("No CORS origins configured!");
                policy.SetIsOriginAllowed(_ => false);
            }
        });
    });

    // Auth - JWKS validation from MockAuthCenter
    builder.Services.AddGatewayAuth(builder.Configuration);

    // YARP Reverse Proxy - code-based configuration from Domeo.Shared.Routes
    builder.Services
        .AddReverseProxy()
        .LoadFromMemory(
            GatewayRouteBuilder.BuildRoutes(),
            GatewayClusterBuilder.BuildClusters(builder.Configuration));

    // OpenAPI
    builder.Services.AddOpenApi();

    // Health Checks
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // CORS must be very early in pipeline (before any other middleware that might short-circuit)
    app.UseCors(corsPolicyName);

    // Global Exception Handler
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
    app.UseClaimsForwarding(); // Forward user claims to microservices

    // Endpoints
    app.MapHealthChecks("/health");
    app.MapReverseProxy();

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
