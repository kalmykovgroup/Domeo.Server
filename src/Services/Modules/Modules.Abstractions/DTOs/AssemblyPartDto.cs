using Modules.Abstractions.Entities.Shared;

namespace Modules.Abstractions.DTOs;

public sealed record AssemblyPartDto(
    Guid Id,
    Guid AssemblyId,
    Guid ComponentId,
    string Role,
    DynamicSize? Length,
    DynamicSize? Width,
    Placement Placement,
    int Quantity,
    string? QuantityFormula,
    int SortOrder);
