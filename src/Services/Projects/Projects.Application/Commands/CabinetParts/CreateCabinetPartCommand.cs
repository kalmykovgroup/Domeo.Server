using MediatR;
using Modules.Domain.Entities.Shared;

namespace Projects.Application.Commands.CabinetParts;

public sealed record CreateCabinetPartCommand(
    Guid CabinetId,
    Guid ComponentId,
    Guid? SourceAssemblyPartId,
    string? X,
    string? Y,
    string? Z,
    double RotationX,
    double RotationY,
    double RotationZ,
    List<ShapeSegment>? Shape,
    string? Condition,
    int Quantity,
    string? QuantityFormula,
    int SortOrder,
    bool IsEnabled,
    Guid? MaterialId,
    Dictionary<string, string>? Provides) : IRequest<Guid>;
