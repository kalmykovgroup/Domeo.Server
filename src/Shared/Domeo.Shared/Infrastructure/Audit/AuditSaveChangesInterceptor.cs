using System.Text.Json;
using Domeo.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Domeo.Shared.Infrastructure.Audit;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IAuditContextAccessor _auditContextAccessor;
    private readonly string _serviceName;

    public AuditSaveChangesInterceptor(
        IEventPublisher eventPublisher,
        IAuditContextAccessor auditContextAccessor,
        string serviceName)
    {
        _eventPublisher = eventPublisher;
        _auditContextAccessor = auditContextAccessor;
        _serviceName = serviceName;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditContext = _auditContextAccessor.AuditContext;
        if (auditContext is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var entries = eventData.Context.ChangeTracker
            .Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry);
            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => "Unknown"
            };

            string? oldValue = null;
            string? newValue = null;

            if (entry.State == EntityState.Modified)
            {
                var originalValues = entry.OriginalValues.Properties
                    .ToDictionary(p => p.Name, p => entry.OriginalValues[p]);
                var currentValues = entry.CurrentValues.Properties
                    .ToDictionary(p => p.Name, p => entry.CurrentValues[p]);

                oldValue = JsonSerializer.Serialize(originalValues);
                newValue = JsonSerializer.Serialize(currentValues);
            }
            else if (entry.State == EntityState.Added)
            {
                var currentValues = entry.CurrentValues.Properties
                    .ToDictionary(p => p.Name, p => entry.CurrentValues[p]);
                newValue = JsonSerializer.Serialize(currentValues);
            }
            else if (entry.State == EntityState.Deleted)
            {
                var originalValues = entry.OriginalValues.Properties
                    .ToDictionary(p => p.Name, p => entry.OriginalValues[p]);
                oldValue = JsonSerializer.Serialize(originalValues);
            }

            var auditEvent = new AuditEvent
            {
                UserId = auditContext.UserId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                ServiceName = _serviceName,
                OldValue = oldValue,
                NewValue = newValue,
                IpAddress = auditContext.IpAddress,
                CorrelationId = auditContext.CorrelationId
            };

            await _eventPublisher.PublishAuditAsync(auditEvent, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static string GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
        if (keyProperties is null || keyProperties.Count == 0)
            return "unknown";

        var keyValues = keyProperties
            .Select(p => entry.Property(p.Name).CurrentValue?.ToString())
            .Where(v => v is not null);

        return string.Join("-", keyValues);
    }
}
