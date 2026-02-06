using Modules.Domain.Entities.Shared;

namespace Modules.Contracts.DTOs.AssemblyParts;

public sealed record UpdateAssemblyPartRequest(
    Guid ComponentId,
    PartRole Role,
    Placement Placement,
    DynamicSize? Length,
    DynamicSize? Width,
    List<ShapeSegment>? Shape,
    int Quantity,
    string? QuantityFormula,
    int SortOrder);
