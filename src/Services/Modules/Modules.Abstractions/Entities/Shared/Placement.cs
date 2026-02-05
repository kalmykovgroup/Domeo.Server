namespace Modules.Abstractions.Entities.Shared;

public sealed record Placement(
    AnchorOrigin AnchorX,
    AnchorOrigin AnchorY,
    AnchorOrigin AnchorZ,
    double OffsetX,
    double OffsetY,
    double OffsetZ,
    double RotationX,
    double RotationY,
    double RotationZ);
