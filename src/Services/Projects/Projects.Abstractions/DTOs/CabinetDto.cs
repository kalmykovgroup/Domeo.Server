namespace Projects.Abstractions.DTOs;

public sealed record CabinetDto(
    Guid Id,
    Guid RoomId,
    Guid? EdgeId,
    Guid? ZoneId,
    int? ModuleTypeId,
    string? Name,
    string PlacementType,
    string? FacadeType,
    double PositionX,
    double PositionY,
    double Rotation,
    double Width,
    double Height,
    double Depth,
    decimal? CalculatedPrice,
    DateTime CreatedAt);
