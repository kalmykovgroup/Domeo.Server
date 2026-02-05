using System.Text.Json;
using Audit.Abstractions.Entities;
using Audit.API.Infrastructure.Persistence;
using Domeo.Shared.Events;
using Domeo.Shared.Infrastructure.Redis;
using Domeo.Shared.Infrastructure.Resilience;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Audit.API.Services;

public sealed class AuditEventSubscriber : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAuditEventBuffer _buffer;
    private readonly IConnectionStateTracker _stateTracker;
    private readonly ILogger<AuditEventSubscriber> _logger;
    private readonly string _redisConnectionString;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AuditEventSubscriber(
        IServiceScopeFactory scopeFactory,
        IAuditEventBuffer buffer,
        IConnectionStateTracker stateTracker,
        IConfiguration configuration,
        ILogger<AuditEventSubscriber> logger)
    {
        _scopeFactory = scopeFactory;
        _buffer = buffer;
        _stateTracker = stateTracker;
        _logger = logger;

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        if (!redisConnectionString.Contains("abortConnect", StringComparison.OrdinalIgnoreCase))
        {
            redisConnectionString = redisConnectionString.TrimEnd(',', ';') + ",abortConnect=false";
        }
        _redisConnectionString = redisConnectionString;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuditEventSubscriber starting, connecting to Redis...");

        try
        {
            var connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString);
            var subscriber = connection.GetSubscriber();

            await subscriber.SubscribeAsync(
                RedisChannel.Literal(RedisEventPublisher.AuditChannel),
                async (channel, message) => await HandleAuditEventAsync(message));

            await subscriber.SubscribeAsync(
                RedisChannel.Literal(RedisEventPublisher.SessionChannel),
                async (channel, message) => await HandleSessionEventAsync(message));

            await subscriber.SubscribeAsync(
                RedisChannel.Literal(RedisEventPublisher.ErrorChannel),
                async (channel, message) => await HandleErrorEventAsync(message));

            _logger.LogInformation(
                "AuditEventSubscriber subscribed to channels: {AuditChannel}, {SessionChannel}, {ErrorChannel}",
                RedisEventPublisher.AuditChannel,
                RedisEventPublisher.SessionChannel,
                RedisEventPublisher.ErrorChannel);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Could not connect to Redis. Audit event subscription disabled");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("AuditEventSubscriber stopping");
        }
    }

    private async Task HandleAuditEventAsync(RedisValue message)
    {
        if (message.IsNullOrEmpty)
            return;

        try
        {
            var auditEvent = JsonSerializer.Deserialize<AuditEvent>(message.ToString(), JsonOptions);
            if (auditEvent is null)
                return;

            // If database is available, write directly
            if (_stateTracker.IsDatabaseAvailable)
            {
                await ProcessAuditEventDirectlyAsync(auditEvent);
            }
            else
            {
                // Otherwise, buffer for later
                await _buffer.EnqueueAuditAsync(auditEvent);
                _logger.LogDebug("Audit event buffered (DB unavailable): {Action} on {EntityType}",
                    auditEvent.Action, auditEvent.EntityType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audit event");
        }
    }

    private async Task HandleSessionEventAsync(RedisValue message)
    {
        if (message.IsNullOrEmpty)
            return;

        try
        {
            var wrapper = JsonSerializer.Deserialize<SessionEventWrapper>(message.ToString(), JsonOptions);
            if (wrapper is null)
                return;

            // If database is available, write directly
            if (_stateTracker.IsDatabaseAvailable)
            {
                await ProcessSessionEventDirectlyAsync(wrapper);
            }
            else
            {
                // Otherwise, buffer for later
                await BufferSessionEventAsync(wrapper);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing session event");
        }
    }

    private async Task BufferSessionEventAsync(SessionEventWrapper wrapper)
    {
        switch (wrapper.EventType)
        {
            case nameof(UserLoggedInEvent):
                var loginEvent = JsonSerializer.Deserialize<UserLoggedInEvent>(wrapper.Payload, JsonOptions);
                if (loginEvent is not null)
                {
                    await _buffer.EnqueueSessionAsync(loginEvent, wrapper.EventType);
                    _logger.LogDebug("Login event buffered (DB unavailable): {UserId}", loginEvent.UserId);
                }
                break;

            case nameof(UserLoggedOutEvent):
                var logoutEvent = JsonSerializer.Deserialize<UserLoggedOutEvent>(wrapper.Payload, JsonOptions);
                if (logoutEvent is not null)
                {
                    await _buffer.EnqueueSessionAsync(logoutEvent, wrapper.EventType);
                    _logger.LogDebug("Logout event buffered (DB unavailable): {UserId}", logoutEvent.UserId);
                }
                break;

            default:
                _logger.LogWarning("Unknown session event type: {EventType}", wrapper.EventType);
                break;
        }
    }

    private async Task ProcessAuditEventDirectlyAsync(AuditEvent auditEvent)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        var auditLog = AuditLog.Create(
            auditEvent.UserId,
            auditEvent.Action,
            auditEvent.EntityType,
            auditEvent.EntityId,
            auditEvent.ServiceName,
            auditEvent.OldValue,
            auditEvent.NewValue,
            auditEvent.IpAddress);

        dbContext.AuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync();

        _logger.LogDebug("Audit event saved: {Action} on {EntityType}/{EntityId}",
            auditEvent.Action, auditEvent.EntityType, auditEvent.EntityId);
    }

    private async Task ProcessSessionEventDirectlyAsync(SessionEventWrapper wrapper)
    {
        switch (wrapper.EventType)
        {
            case nameof(UserLoggedInEvent):
                var loginEvent = JsonSerializer.Deserialize<UserLoggedInEvent>(wrapper.Payload, JsonOptions);
                if (loginEvent is not null)
                    await ProcessLoginEventAsync(loginEvent);
                break;

            case nameof(UserLoggedOutEvent):
                var logoutEvent = JsonSerializer.Deserialize<UserLoggedOutEvent>(wrapper.Payload, JsonOptions);
                if (logoutEvent is not null)
                    await ProcessLogoutEventAsync(logoutEvent);
                break;

            default:
                _logger.LogWarning("Unknown session event type: {EventType}", wrapper.EventType);
                break;
        }
    }

    private async Task ProcessLoginEventAsync(UserLoggedInEvent loginEvent)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        var session = LoginSession.Create(
            loginEvent.SessionId,
            loginEvent.UserId,
            loginEvent.UserRole,
            loginEvent.IpAddress,
            loginEvent.UserAgent);

        dbContext.LoginSessions.Add(session);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Login session created: {SessionId} for user {UserId}",
            loginEvent.SessionId, loginEvent.UserId);
    }

    private async Task ProcessLogoutEventAsync(UserLoggedOutEvent logoutEvent)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        var session = await dbContext.LoginSessions
            .FirstOrDefaultAsync(s => s.Id == logoutEvent.SessionId);

        if (session is null)
        {
            _logger.LogWarning("Login session not found for logout: {SessionId}", logoutEvent.SessionId);
            return;
        }

        session.Logout();
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Login session closed: {SessionId} for user {UserId}",
            logoutEvent.SessionId, logoutEvent.UserId);
    }

    private async Task HandleErrorEventAsync(RedisValue message)
    {
        if (message.IsNullOrEmpty)
            return;

        try
        {
            var errorEvent = JsonSerializer.Deserialize<ApplicationErrorEvent>(message.ToString(), JsonOptions);
            if (errorEvent is null)
                return;

            if (_stateTracker.IsDatabaseAvailable)
            {
                await ProcessErrorEventDirectlyAsync(errorEvent);
            }
            else
            {
                _logger.LogDebug("Error event received but DB unavailable: {ServiceName} - {Message}",
                    errorEvent.ServiceName, errorEvent.Message);
            }
        }
        catch (JsonException ex)
        {
            // Log as Warning to avoid infinite loop (Error level would trigger RedisSink -> publish -> handle -> error)
            _logger.LogWarning("Failed to deserialize error event: {Error}. Message: {Message}",
                ex.Message, message.ToString().Length > 200 ? message.ToString()[..200] + "..." : message.ToString());
        }
        catch (Exception ex)
        {
            // Log as Warning to avoid infinite loop
            _logger.LogWarning(ex, "Error processing error event");
        }
    }

    private async Task ProcessErrorEventDirectlyAsync(ApplicationErrorEvent errorEvent)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        var applicationLog = ApplicationLog.Create(
            errorEvent.ServiceName,
            errorEvent.Level,
            errorEvent.Message,
            errorEvent.Exception,
            errorEvent.ExceptionType,
            errorEvent.Properties,
            errorEvent.RequestPath,
            errorEvent.UserId,
            errorEvent.CorrelationId);

        dbContext.ApplicationLogs.Add(applicationLog);
        await dbContext.SaveChangesAsync();

        _logger.LogDebug("Application error saved: {ServiceName} - {Level} - {Message}",
            errorEvent.ServiceName, errorEvent.Level, errorEvent.Message);
    }
}
