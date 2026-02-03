using Domeo.Shared.Infrastructure.Audit;
using Domeo.Shared.Infrastructure.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Domeo.Shared.Infrastructure;

public static class DependencyInjection
{
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
