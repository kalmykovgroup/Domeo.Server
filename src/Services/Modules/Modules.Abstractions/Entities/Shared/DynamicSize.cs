namespace Modules.Abstractions.Entities.Shared;

public sealed record DynamicSize(DimensionSource Source, double Offset, double? FixedValue);
