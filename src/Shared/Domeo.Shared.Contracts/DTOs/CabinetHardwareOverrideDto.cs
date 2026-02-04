namespace Domeo.Shared.Contracts.DTOs;

public sealed record CabinetHardwareOverrideDto(
    Guid Id,
    Guid CabinetId,
    int ModuleHardwareId,
    int? HardwareId,
    string? Role,
    string? QuantityFormula,
    string? PositionXFormula,
    string? PositionYFormula,
    string? PositionZFormula,
    bool IsEnabled,
    string? MaterialId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
