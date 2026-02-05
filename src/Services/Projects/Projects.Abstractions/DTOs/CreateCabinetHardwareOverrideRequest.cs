namespace Projects.Abstractions.DTOs;

public sealed record CreateCabinetHardwareOverrideRequest(
    Guid AssemblyPartId,
    bool IsEnabled = true,
    Guid? ComponentId = null,
    string? Role = null,
    string? QuantityFormula = null,
    string? PositionXFormula = null,
    string? PositionYFormula = null,
    string? PositionZFormula = null,
    string? MaterialId = null);
