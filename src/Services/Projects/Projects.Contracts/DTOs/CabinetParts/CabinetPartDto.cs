using Modules.Domain.Entities.Shared;

namespace Projects.Contracts.DTOs.CabinetParts;

public sealed record CabinetPartDto(
    Guid Id,
    Guid CabinetId,
    Guid? SourceAssemblyPartId,
    Guid ComponentId,
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
    Dictionary<string, string>? Provides,
    DateTime CreatedAt);
