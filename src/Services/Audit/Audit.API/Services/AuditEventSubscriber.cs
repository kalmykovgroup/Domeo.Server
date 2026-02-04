using System.Text.Json;
using Audit.API.Entities;
using Audit.API.Persistence;
using Domeo.Shared.Contracts.Events;
using Domeo.Shared.Infrastructure.Redis;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Audit.API.Services;

public sealed class AuditEventSubscriber : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditEventSubscriber> _logger;
    private readonly string _redisConnectionString;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditEventSubscriber(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<AuditEventSubscriber> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuditEventSubscriber starting, connecting to Redis...");

        try
        {
            var connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString);
            var subscriber = connection.GetSubscriber();

            // Subscribe to entity audit events
            await subscriber.SubscribeAsync(
                RedisChannel.Literal(RedisEventPublisher.AuditChannel),
                async (channel, message) => await HandleAuditEventAsync(message));

            // Subscribe to session events
            await subscriber.SubscribeAsync(
                RedisChannel.Literal(RedisEventPublisher.SessionChannel),
                async (channel, message) => await HandleSessionEventAsync(message));

            _logger.LogInformation(
                "AuditEventSubscriber subscribed to channels: {AuditChannel}, {SessionChannel}",
                RedisEventPublisher.AuditChannel,
                RedisEventPublisher.SessionChannel);

            // Keep the service running
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

            await ProcessAuditEventAsync(auditEvent);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing session event");
        }
    }

    private async Task ProcessAuditEventAsync(AuditEvent auditEvent)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        var auditLog = AuditLog.Create(
            auditEvent.UserId,
            auditEvent.UserEmail,
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

    private async Task ProcessLoginEventAsync(UserLoggedInEvent loginEvent)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        var session = LoginSession.Create(
            loginEvent.SessionId,
            loginEvent.UserId,
            loginEvent.UserEmail,
            loginEvent.UserName,
            loginEvent.UserRole,
            loginEvent.IpAddress,
            loginEvent.UserAgent);

        dbContext.LoginSessions.Add(session);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Login session created: {SessionId} for user {UserEmail}",
            loginEvent.SessionId, loginEvent.UserEmail);
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

        _logger.LogInformation("Login session closed: {SessionId} for user {UserEmail}",
            logoutEvent.SessionId, logoutEvent.UserEmail);
    }
}
