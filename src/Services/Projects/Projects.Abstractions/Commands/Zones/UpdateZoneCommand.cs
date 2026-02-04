using MediatR;

namespace Projects.Abstractions.Commands.Zones;

public sealed record UpdateZoneCommand(
    Guid EdgeId,
    Guid ZoneId,
    string? Name,
    double StartX,
    double EndX) : IRequest;
