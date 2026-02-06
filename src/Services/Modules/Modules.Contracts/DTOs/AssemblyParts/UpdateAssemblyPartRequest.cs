using Modules.Domain.Entities.Shared;

namespace Modules.Contracts.DTOs.AssemblyParts;

public sealed record UpdateAssemblyPartRequest(
    Guid ComponentId,
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
    Dictionary<string, string>? Provides = null);
