namespace Domeo.Shared.Kernel.Domain.Abstractions;

/// <summary>
/// Base class for entities with audit fields (CreatedAt, UpdatedAt)
/// </summary>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity
    where TId : notnull
{
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected AuditableEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void SetCreatedAt(DateTime createdAt)
    {
        CreatedAt = createdAt;
    }

    public void SetUpdatedAt(DateTime? updatedAt)
    {
        UpdatedAt = updatedAt;
    }
}
