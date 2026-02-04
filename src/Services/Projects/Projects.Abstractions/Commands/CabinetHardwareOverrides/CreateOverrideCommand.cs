using MediatR;

namespace Projects.Abstractions.Commands.CabinetHardwareOverrides;

public sealed record CreateOverrideCommand(
    Guid CabinetId,
    int ModuleHardwareId,
    bool IsEnabled,
    int? HardwareId,
    string? Role,
    string? QuantityFormula,
    string? PositionXFormula,
    string? PositionYFormula,
    string? PositionZFormula,
    string? MaterialId) : IRequest<Guid>;
