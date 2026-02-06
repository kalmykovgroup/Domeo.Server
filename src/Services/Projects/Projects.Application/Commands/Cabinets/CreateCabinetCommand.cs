using MediatR;
using Projects.Contracts.DTOs.CabinetParts;

namespace Projects.Application.Commands.Cabinets;

public sealed record CreateCabinetCommand(
    Guid RoomId,
    string PlacementType,
    double PositionX,
    double PositionY,
    Dictionary<string, double>? ParameterOverrides,
    string? Name,
    Guid? EdgeId,
    Guid? ZoneId,
    Guid? AssemblyId,
    string? FacadeType,
    double Rotation,
    List<CreateCabinetPartRequest>? Parts) : IRequest<Guid>;
