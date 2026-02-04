using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Modules.API.Entities;

public sealed class ModuleCategory : Entity<string>
{
    public string? ParentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int OrderIndex { get; private set; }
    public bool IsActive { get; private set; } = true;

    private ModuleCategory() { }

    public static ModuleCategory Create(
        string id,
        string name,
        string? parentId = null,
        string? description = null,
        int orderIndex = 0)
    {
        return new ModuleCategory
        {
            Id = id,
            Name = name,
            ParentId = parentId,
            Description = description,
            OrderIndex = orderIndex,
            IsActive = true
        };
    }

    public void Update(string name, string? description, int orderIndex)
    {
        Name = name;
        Description = description;
        OrderIndex = orderIndex;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
