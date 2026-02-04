namespace Projects.Abstractions.DTOs;

public sealed record CreateCabinetRequest(
    Guid RoomId,
    string PlacementType,
    double PositionX,
    double PositionY,
    double Width,
    double Height,
    double Depth,
    string? Name,
    Guid? EdgeId,
    Guid? ZoneId,
    int? ModuleTypeId,
    string? FacadeType,
    double Rotation = 0);
