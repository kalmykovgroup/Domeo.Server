using MediatR;
using Projects.Contracts.DTOs.Zones;

namespace Projects.Application.Commands.Zones;

public sealed record GetZonesQuery(Guid EdgeId) : IRequest<List<ZoneDto>>;
