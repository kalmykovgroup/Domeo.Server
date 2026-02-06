using MediatR;
using Modules.Contracts.DTOs.AssemblyParts;
using Modules.Domain.Entities.Shared;

namespace Modules.Application.Commands.AssemblyParts;

public sealed record CreateAssemblyPartCommand(
    Guid AssemblyId,
    Guid ComponentId,
    string? LengthExpr,
    string? WidthExpr,
    string? X,
    string? Y,
    string? Z,
    double RotationX,
    double RotationY,
    double RotationZ,
    string? Condition,
    List<ShapeSegment>? Shape,
    int Quantity,
    string? QuantityFormula,
    int SortOrder) : IRequest<AssemblyPartDto>;
