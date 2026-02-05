using MediatR;
using Modules.Contracts.DTOs.AssemblyParts;

namespace Modules.Application.Queries.AssemblyParts;

public sealed record GetAssemblyPartsQuery(Guid AssemblyId) : IRequest<List<AssemblyPartDto>>;
