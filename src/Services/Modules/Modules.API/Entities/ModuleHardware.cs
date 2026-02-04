using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Modules.API.Entities;

public sealed class ModuleHardware : Entity<int>
{
    public int ModuleTypeId { get; private set; }
    public int HardwareId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public string QuantityFormula { get; private set; } = "1";
    public string? PositionXFormula { get; private set; }
    public string? PositionYFormula { get; private set; }
    public string? PositionZFormula { get; private set; }

    private ModuleHardware() { }

    public static ModuleHardware Create(
        int id,
        int moduleTypeId,
        int hardwareId,
        string role,
        string quantityFormula = "1",
        string? positionXFormula = null,
        string? positionYFormula = null,
        string? positionZFormula = null)
    {
        return new ModuleHardware
        {
            Id = id,
            ModuleTypeId = moduleTypeId,
            HardwareId = hardwareId,
            Role = role,
            QuantityFormula = quantityFormula,
            PositionXFormula = positionXFormula,
            PositionYFormula = positionYFormula,
            PositionZFormula = positionZFormula
        };
    }
}
