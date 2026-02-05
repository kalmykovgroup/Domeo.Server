using MediatR;

namespace Projects.Abstractions.Commands.CabinetHardwareOverrides;

public sealed record UpdateOverrideCommand(
    Guid Id,
    bool? IsEnabled,
    Guid? ComponentId,
    string? Role,
    string? QuantityFormula,
    string? PositionXFormula,
    string? PositionYFormula,
    string? PositionZFormula,
    string? MaterialId) : IRequest;
