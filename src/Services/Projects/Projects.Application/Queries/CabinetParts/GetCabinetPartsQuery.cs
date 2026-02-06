using MediatR;
using Projects.Contracts.DTOs.CabinetParts;

namespace Projects.Application.Queries.CabinetParts;

public sealed record GetCabinetPartsQuery(Guid CabinetId) : IRequest<List<CabinetPartDto>>;
