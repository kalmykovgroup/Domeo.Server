using Domeo.Shared.Domain;

namespace Projects.Domain.Entities;

public sealed class Room : AuditableEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int CeilingHeight { get; private set; } = 2700;
    public int OrderIndex { get; private set; }

    private Room() { }

    public static Room Create(
        Guid projectId,
        string name,
        int ceilingHeight = 2700,
        int orderIndex = 0)
    {
        return new Room
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = name,
            CeilingHeight = ceilingHeight,
            OrderIndex = orderIndex
        };
    }

    public void Update(string name, int ceilingHeight, int orderIndex)
    {
        Name = name;
        CeilingHeight = ceilingHeight;
        OrderIndex = orderIndex;
    }
}
