namespace Modules.Domain.Entities.Shared;

public sealed record DynamicSize(DimensionSource Source, double Offset, double? FixedValue);
