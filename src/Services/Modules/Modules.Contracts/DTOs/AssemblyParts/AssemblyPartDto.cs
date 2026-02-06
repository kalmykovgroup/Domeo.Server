using Modules.Contracts.DTOs.Components;
using Modules.Domain.Entities.Shared;

namespace Modules.Contracts.DTOs.AssemblyParts;

public sealed record AssemblyPartDto(
    Guid Id,
    Guid AssemblyId,
    Guid ComponentId,
    string Role,
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
    int SortOrder,
    ComponentDto? Component);
