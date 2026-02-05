using Materials.Contracts.DTOs;
using MediatR;

namespace Materials.Application.Queries.Materials;

public sealed record GetMaterialByIdQuery(string Id) : IRequest<MaterialDto>;
