namespace Projects.Contracts.DTOs.Cabinets;

public sealed record CabinetDto(
    Guid Id,
    Guid RoomId,
    Guid? EdgeId,
    Guid? ZoneId,
    Guid? AssemblyId,
    string? Name,
    string PlacementType,
    string? FacadeType,
    double PositionX,
    double PositionY,
    double Rotation,
    Dictionary<string, double>? ParameterOverrides,
    decimal? CalculatedPrice,
    DateTime CreatedAt);
