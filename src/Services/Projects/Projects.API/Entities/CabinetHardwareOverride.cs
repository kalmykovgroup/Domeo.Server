using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Projects.API.Entities;

/// <summary>
/// Переопределение фурнитуры для конкретного экземпляра шкафа.
/// Позволяет изменить позицию, отключить компонент или назначить материал.
/// </summary>
public sealed class CabinetHardwareOverride : AuditableEntity<Guid>
{
    public Guid CabinetId { get; private set; }
    public int ModuleHardwareId { get; private set; }

    // Переопределения полей из module_hardware (null = использовать оригинал)
    public int? HardwareId { get; private set; }
    public string? Role { get; private set; }
    public string? QuantityFormula { get; private set; }
    public string? PositionXFormula { get; private set; }
    public string? PositionYFormula { get; private set; }
    public string? PositionZFormula { get; private set; }

    // Новые поля (только в overrides)
    public bool IsEnabled { get; private set; } = true;
    public string? MaterialId { get; private set; }

    private CabinetHardwareOverride() { }

    public static CabinetHardwareOverride Create(
        Guid cabinetId,
        int moduleHardwareId,
        bool isEnabled = true,
        int? hardwareId = null,
        string? role = null,
        string? quantityFormula = null,
        string? positionXFormula = null,
        string? positionYFormula = null,
        string? positionZFormula = null,
        string? materialId = null)
    {
        return new CabinetHardwareOverride
        {
            Id = Guid.NewGuid(),
            CabinetId = cabinetId,
            ModuleHardwareId = moduleHardwareId,
            IsEnabled = isEnabled,
            HardwareId = hardwareId,
            Role = role,
            QuantityFormula = quantityFormula,
            PositionXFormula = positionXFormula,
            PositionYFormula = positionYFormula,
            PositionZFormula = positionZFormula,
            MaterialId = materialId
        };
    }

    public void SetEnabled(bool isEnabled) => IsEnabled = isEnabled;

    public void SetHardware(int? hardwareId) => HardwareId = hardwareId;

    public void SetRole(string? role) => Role = role;

    public void SetQuantityFormula(string? formula) => QuantityFormula = formula;

    public void SetPositionFormulas(string? x, string? y, string? z)
    {
        PositionXFormula = x;
        PositionYFormula = y;
        PositionZFormula = z;
    }

    public void SetMaterial(string? materialId) => MaterialId = materialId;

    public void Update(
        bool? isEnabled,
        int? hardwareId,
        string? role,
        string? quantityFormula,
        string? positionXFormula,
        string? positionYFormula,
        string? positionZFormula,
        string? materialId)
    {
        if (isEnabled.HasValue)
            IsEnabled = isEnabled.Value;
        if (hardwareId.HasValue)
            HardwareId = hardwareId;
        if (role != null)
            Role = role;
        if (quantityFormula != null)
            QuantityFormula = quantityFormula;
        if (positionXFormula != null)
            PositionXFormula = positionXFormula;
        if (positionYFormula != null)
            PositionYFormula = positionYFormula;
        if (positionZFormula != null)
            PositionZFormula = positionZFormula;
        if (materialId != null)
            MaterialId = materialId;
    }
}
