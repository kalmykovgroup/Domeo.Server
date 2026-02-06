using Modules.Contracts.DTOs.Components;
using Modules.Domain.Entities.Shared;

namespace Modules.Contracts.DTOs.AssemblyParts;

public sealed record AssemblyPartDto(
    Guid Id,
    Guid AssemblyId,
    Guid ComponentId,
    PartRole Role,
    DynamicSize? Length,
    DynamicSize? Width,
    Placement Placement,
    List<ShapeSegment>? Shape,
    int Quantity,
    string? QuantityFormula,
    int SortOrder,
    ComponentDto? Component);
