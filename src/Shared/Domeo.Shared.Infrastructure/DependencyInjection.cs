using Domeo.Shared.Infrastructure.Audit;
using Domeo.Shared.Infrastructure.HealthChecks;
using Domeo.Shared.Infrastructure.Redis;
using Domeo.Shared.Infrastructure.Resilience;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Domeo.Shared.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Adds shared infrastructure with resilience support for graceful degradation
    /// </summary>
    public static IServiceCollection AddResilientInfrastructure<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
        where TDbContext : DbContext
    {
        // Configure resilience options
        services.Configure<ResilienceOptions>(
            configuration.GetSection(ResilienceOptions.SectionName));

        // Register connection state tracker as singleton
        services.AddSingleton<IConnectionStateTracker, ConnectionStateTracker>();

        // Configure Redis with graceful fallback
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

        if (!redisConnectionString.Contains("abortConnect", StringComparison.OrdinalIgnoreCase))
        {
            redisConnectionString = redisConnectionString.TrimEnd(',', ';') + ",abortConnect=false";
        }

        IConnectionMultiplexer? redisConnection = null;
        try
        {
            redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
            if (redisConnection.IsConnected)
            {
                services.AddSingleton(redisConnection);
            }
        }
        catch
        {
            // Redis unavailable at startup - will monitor and reconnect
        }

        // Fallback event store for when Redis is unavailable
        services.AddSingleton<FallbackEventStore>();

        // Always use resilient event publisher with fallback support
        services.AddSingleton<IEventPublisher, ResilientEventPublisher>();

        // Add health monitor services
        services.AddHostedService<DatabaseHealthMonitorService<TDbContext>>();
        services.AddHostedService<RedisHealthMonitorService>();

        // Add fallback event replay service (replays events when Redis becomes available)
        services.AddHostedService<FallbackEventReplayService>();

        // Add degraded health checks
        services.AddHealthChecks()
            .AddCheck<DegradedDatabaseHealthCheck>("database")
            .AddCheck<DegradedRedisHealthCheck>("redis");

        // Audit infrastructure
        services.AddSingleton<IAuditContextAccessor, AuditContextAccessor>();
        services.AddScoped(sp => new AuditSaveChangesInterceptor(
            sp.GetRequiredService<IEventPublisher>(),
            sp.GetRequiredService<IAuditContextAccessor>(),
            serviceName));

        return services;
    }

    /// <summary>
    /// Adds minimal infrastructure for services without database (e.g., API Gateway)
    /// </summary>
    public static IServiceCollection AddMinimalInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register connection state tracker as singleton
        services.AddSingleton<IConnectionStateTracker, ConnectionStateTracker>();

        // Configure Redis with graceful fallback
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

        if (!redisConnectionString.Contains("abortConnect", StringComparison.OrdinalIgnoreCase))
        {
            redisConnectionString = redisConnectionString.TrimEnd(',', ';') + ",abortConnect=false";
        }

        try
        {
            var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
            if (redisConnection.IsConnected)
            {
                services.AddSingleton<IConnectionMultiplexer>(redisConnection);
            }
        }
        catch
        {
            // Redis unavailable at startup
        }

        // Fallback event store for when Redis is unavailable
        services.AddSingleton<FallbackEventStore>();

        // Resilient event publisher with fallback support
        services.AddSingleton<IEventPublisher, ResilientEventPublisher>();

        // Add Redis health monitor
        services.AddHostedService<RedisHealthMonitorService>();

        // Add fallback event replay service
        services.AddHostedService<FallbackEventReplayService>();

        return services;
    }

    /// <summary>
    /// Legacy method - adds shared infrastructure without resilience monitoring
    /// </summary>
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

        // Add AbortOnConnectFail=false to allow graceful degradation
        if (!redisConnectionString.Contains("abortConnect", StringComparison.OrdinalIgnoreCase))
        {
            redisConnectionString = redisConnectionString.TrimEnd(',', ';') + ",abortConnect=false";
        }

        // Try to connect to Redis with graceful fallback
        IConnectionMultiplexer? redisConnection = null;
        try
        {
            redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
        }
        catch
        {
            // Redis unavailable - will use NoOp publisher
        }

        if (redisConnection?.IsConnected == true)
        {
            services.AddSingleton(redisConnection);
            services.AddSingleton<IEventPublisher, RedisEventPublisher>();
        }
        else
        {
            services.AddSingleton<IEventPublisher, NoOpEventPublisher>();
        }

        services.AddSingleton<IAuditContextAccessor, AuditContextAccessor>();

        services.AddScoped(sp => new AuditSaveChangesInterceptor(
            sp.GetRequiredService<IEventPublisher>(),
            sp.GetRequiredService<IAuditContextAccessor>(),
            serviceName));

        return services;
    }
}
