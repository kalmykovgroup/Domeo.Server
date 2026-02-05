using Domeo.Shared.Domain;
using Modules.Domain.Entities.Shared;

namespace Modules.Domain.Entities;

public sealed class Component : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public ComponentParams? Params { get; private set; }
    public List<string> Tags { get; private set; } = [];
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private Component() { }

    public static Component Create(
        string name,
        ComponentParams? @params = null,
        List<string>? tags = null)
    {
        return new Component
        {
            Id = Guid.NewGuid(),
            Name = name,
            Params = @params,
            Tags = tags ?? [],
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, ComponentParams? @params, List<string>? tags)
    {
        Name = name;
        Params = @params;
        if (tags is not null) Tags = tags;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
