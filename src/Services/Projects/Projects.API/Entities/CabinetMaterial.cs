using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Projects.API.Entities;

public sealed class CabinetMaterial : Entity<Guid>
{
    public Guid CabinetId { get; private set; }
    public string MaterialType { get; private set; } = string.Empty;
    public Guid MaterialId { get; private set; }

    private CabinetMaterial() { }

    public static CabinetMaterial Create(
        Guid cabinetId,
        string materialType,
        Guid materialId)
    {
        return new CabinetMaterial
        {
            Id = Guid.NewGuid(),
            CabinetId = cabinetId,
            MaterialType = materialType,
            MaterialId = materialId
        };
    }

    public void UpdateMaterial(Guid materialId)
    {
        MaterialId = materialId;
    }
}
