using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Audit.Abstractions.Entities;

public sealed class AuditLog : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string UserEmail { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string ServiceName { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid userId,
        string userEmail,
        string action,
        string entityType,
        string entityId,
        string serviceName,
        string? oldValue = null,
        string? newValue = null,
        string? ipAddress = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            ServiceName = serviceName,
            OldValue = oldValue,
            NewValue = newValue,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
    }
}
