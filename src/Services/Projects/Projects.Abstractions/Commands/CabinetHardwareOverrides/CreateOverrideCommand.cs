using MediatR;

namespace Projects.Abstractions.Commands.CabinetHardwareOverrides;

public sealed record CreateOverrideCommand(
    Guid CabinetId,
    Guid AssemblyPartId,
    bool IsEnabled,
    Guid? ComponentId,
    string? Role,
    string? QuantityFormula,
    string? PositionXFormula,
    string? PositionYFormula,
    string? PositionZFormula,
    string? MaterialId) : IRequest<Guid>;
