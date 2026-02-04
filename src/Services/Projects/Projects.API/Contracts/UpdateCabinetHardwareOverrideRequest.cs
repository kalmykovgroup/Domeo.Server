namespace Projects.API.Contracts;

public sealed record UpdateCabinetHardwareOverrideRequest(
    bool? IsEnabled = null,
    int? HardwareId = null,
    string? Role = null,
    string? QuantityFormula = null,
    string? PositionXFormula = null,
    string? PositionYFormula = null,
    string? PositionZFormula = null,
    string? MaterialId = null);
