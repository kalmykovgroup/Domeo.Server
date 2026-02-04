using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Materials.Abstractions.Entities;

public sealed class MaterialCategory : Entity<Guid>
{
    public Guid? ParentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Level { get; private set; }
    public int OrderIndex { get; private set; }
    public bool IsActive { get; private set; } = true;

    private MaterialCategory() { }

    public static MaterialCategory Create(
        Guid id,
        string name,
        Guid? parentId = null,
        int level = 0,
        int orderIndex = 0)
    {
        return new MaterialCategory
        {
            Id = id,
            Name = name,
            ParentId = parentId,
            Level = level,
            OrderIndex = orderIndex,
            IsActive = true
        };
    }

    public void Update(string name, int orderIndex)
    {
        Name = name;
        OrderIndex = orderIndex;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
