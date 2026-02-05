using MediatR;

namespace Projects.Application.Commands.Cabinets;

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
    Guid? AssemblyId,
    string? FacadeType,
    double Rotation) : IRequest<Guid>;
