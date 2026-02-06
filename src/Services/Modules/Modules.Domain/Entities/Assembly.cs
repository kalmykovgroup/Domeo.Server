using Domeo.Shared.Domain;
using Modules.Domain.Entities.Shared;

namespace Modules.Domain.Entities;

public sealed class Assembly : Entity<Guid>
{
    public string CategoryId { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Dictionary<string, double> Parameters { get; private set; } = new();
    public Dictionary<string, ParamConstraint>? ParamConstraints { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private Assembly() { }

    public static Assembly Create(
        string categoryId,
        string type,
        string name,
        Dictionary<string, double> parameters,
        Dictionary<string, ParamConstraint>? paramConstraints = null)
    {
        return new Assembly
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Type = type,
            Name = name,
            Parameters = parameters,
            ParamConstraints = paramConstraints,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        Dictionary<string, double> parameters,
        Dictionary<string, ParamConstraint>? paramConstraints)
    {
        Name = name;
        Parameters = parameters;
        ParamConstraints = paramConstraints;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
