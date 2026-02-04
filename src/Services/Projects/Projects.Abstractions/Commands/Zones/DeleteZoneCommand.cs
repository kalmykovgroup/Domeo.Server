using MediatR;

namespace Projects.Abstractions.Commands.Zones;

public sealed record DeleteZoneCommand(Guid EdgeId, Guid ZoneId) : IRequest;
