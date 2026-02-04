using MediatR;

namespace Projects.Abstractions.Commands.Cabinets;

public sealed record CreateCabinetCommand(
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
    double Rotation) : IRequest<Guid>;
