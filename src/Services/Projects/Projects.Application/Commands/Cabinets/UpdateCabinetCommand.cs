using MediatR;

namespace Projects.Application.Commands.Cabinets;

public sealed record UpdateCabinetCommand(
    Guid Id,
    double PositionX,
    double PositionY,
    double Rotation,
    double Width,
    double Height,
    double Depth,
    string? Name,
    Guid? EdgeId,
    Guid? ZoneId,
    Guid? AssemblyId,
    string? FacadeType,
    decimal? CalculatedPrice) : IRequest;
