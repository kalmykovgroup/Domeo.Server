using MediatR;
using Modules.Abstractions.DTOs;

namespace Modules.Abstractions.Queries.AssemblyParts;

public sealed record GetAssemblyPartsQuery(Guid AssemblyId) : IRequest<List<AssemblyPartDto>>;
