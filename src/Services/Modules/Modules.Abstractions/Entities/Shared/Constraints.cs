namespace Modules.Abstractions.Entities.Shared;

public sealed record Constraints(
    double WidthMin,
    double WidthMax,
    double HeightMin,
    double HeightMax,
    double DepthMin,
    double DepthMax);
