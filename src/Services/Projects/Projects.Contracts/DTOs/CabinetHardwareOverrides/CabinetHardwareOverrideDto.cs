namespace Projects.Contracts.DTOs.CabinetHardwareOverrides;

public sealed record CabinetHardwareOverrideDto(
    Guid Id,
    Guid CabinetId,
    Guid AssemblyPartId,
    Guid? ComponentId,
    string? Role,
    string? QuantityFormula,
    string? PositionXFormula,
    string? PositionYFormula,
    string? PositionZFormula,
    bool IsEnabled,
    string? MaterialId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
