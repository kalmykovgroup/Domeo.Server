using System.Text.Json;
using Audit.API.Entities;
using Audit.API.Persistence;
using Domeo.Shared.Contracts.Events;
using Domeo.Shared.Infrastructure.Resilience;

namespace Audit.API.Services;

public sealed class AuditBufferFlushService : BackgroundService
{
    private readonly IAuditEventBuffer _buffer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionStateTracker _stateTracker;
    private readonly ILogger<AuditBufferFlushService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditBufferFlushService(
        IAuditEventBuffer buffer,
        IServiceScopeFactory scopeFactory,
        IConnectionStateTracker stateTracker,
        ILogger<AuditBufferFlushService> logger)
    {
        _buffer = buffer;
        _scopeFactory = scopeFactory;
        _stateTracker = stateTracker;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit buffer flush service started");

        // Wait for database to become available
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_stateTracker.IsDatabaseAvailable && _buffer.Count > 0)
            {
                await FlushBufferAsync(stoppingToken);
            }

            // Check every 5 seconds
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("Audit buffer flush service stopped");
    }

    private async Task FlushBufferAsync(CancellationToken ct)
    {
        var flushedCount = 0;
        var failedCount = 0;

        _logger.LogInformation("Starting buffer flush. {Count} events in buffer", _buffer.Count);

        await foreach (var bufferedEvent in _buffer.DequeueAllAsync(ct))
        {
            try
            {
                await ProcessBufferedEventAsync(bufferedEvent, ct);
                flushedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process buffered event: {EventType}", bufferedEvent.EventType);
                failedCount++;

                // If database became unavailable, stop flushing
                if (!_stateTracker.IsDatabaseAvailable)
                {
                    _logger.LogWarning("Database became unavailable during flush. Stopping");
                    break;
                }
            }
        }

        if (flushedCount > 0 || failedCount > 0)
        {
            _logger.LogInformation(
                "Buffer flush completed. Flushed: {Flushed}, Failed: {Failed}",
                flushedCount, failedCount);
        }
    }

    private async Task ProcessBufferedEventAsync(BufferedEvent bufferedEvent, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        switch (bufferedEvent.EventType)
        {
            case "AuditEvent":
                var auditEvent = JsonSerializer.Deserialize<AuditEvent>(bufferedEvent.Payload, JsonOptions);
                if (auditEvent is not null)
                {
                    await SaveAuditEventAsync(dbContext, auditEvent, ct);
                }
                break;

            case nameof(UserLoggedInEvent):
                var loginEvent = JsonSerializer.Deserialize<UserLoggedInEvent>(bufferedEvent.Payload, JsonOptions);
                if (loginEvent is not null)
                {
                    await SaveLoginEventAsync(dbContext, loginEvent, ct);
                }
                break;

            case nameof(UserLoggedOutEvent):
                var logoutEvent = JsonSerializer.Deserialize<UserLoggedOutEvent>(bufferedEvent.Payload, JsonOptions);
                if (logoutEvent is not null)
                {
                    await SaveLogoutEventAsync(dbContext, logoutEvent, ct);
                }
                break;

            default:
                _logger.LogWarning("Unknown buffered event type: {EventType}", bufferedEvent.EventType);
                break;
        }
    }

    private static async Task SaveAuditEventAsync(AuditDbContext dbContext, AuditEvent auditEvent, CancellationToken ct)
    {
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
        await dbContext.SaveChangesAsync(ct);
    }

    private static async Task SaveLoginEventAsync(AuditDbContext dbContext, UserLoggedInEvent loginEvent, CancellationToken ct)
    {
        var session = LoginSession.Create(
            loginEvent.SessionId,
            loginEvent.UserId,
            loginEvent.UserEmail,
            loginEvent.UserName,
            loginEvent.UserRole,
            loginEvent.IpAddress,
            loginEvent.UserAgent);

        dbContext.LoginSessions.Add(session);
        await dbContext.SaveChangesAsync(ct);
    }

    private async Task SaveLogoutEventAsync(AuditDbContext dbContext, UserLoggedOutEvent logoutEvent, CancellationToken ct)
    {
        var session = await dbContext.LoginSessions
            .FindAsync([logoutEvent.SessionId], ct);

        if (session is null)
        {
            _logger.LogWarning("Login session not found for logout: {SessionId}", logoutEvent.SessionId);
            return;
        }

        session.Logout();
        await dbContext.SaveChangesAsync(ct);
    }
}
