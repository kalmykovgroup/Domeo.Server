using MediatR;
using Projects.Contracts.DTOs.Cabinets;

namespace Projects.Application.Commands.Cabinets;

public sealed record GetCabinetByIdQuery(Guid Id) : IRequest<CabinetDto>;
