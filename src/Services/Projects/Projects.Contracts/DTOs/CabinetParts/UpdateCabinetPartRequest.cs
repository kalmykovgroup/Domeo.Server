using Modules.Domain.Entities.Shared;

namespace Projects.Contracts.DTOs.CabinetParts;

public sealed record UpdateCabinetPartRequest(
    Guid ComponentId,
    string? X = null,
    string? Y = null,
    string? Z = null,
    double RotationX = 0,
    double RotationY = 0,
    double RotationZ = 0,
    List<ShapeSegment>? Shape = null,
    string? Condition = null,
    int Quantity = 1,
    string? QuantityFormula = null,
    int SortOrder = 0,
    bool IsEnabled = true,
    Guid? MaterialId = null,
    Dictionary<string, string>? Provides = null);
