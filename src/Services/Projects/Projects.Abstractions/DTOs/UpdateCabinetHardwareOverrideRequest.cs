namespace Projects.Abstractions.DTOs;

public sealed record UpdateCabinetHardwareOverrideRequest(
    bool? IsEnabled = null,
    Guid? ComponentId = null,
    string? Role = null,
    string? QuantityFormula = null,
    string? PositionXFormula = null,
    string? PositionYFormula = null,
    string? PositionZFormula = null,
    string? MaterialId = null);
