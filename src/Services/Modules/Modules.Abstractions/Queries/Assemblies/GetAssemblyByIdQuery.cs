using MediatR;
using Modules.Abstractions.DTOs;

namespace Modules.Abstractions.Queries.Assemblies;

public sealed record GetAssemblyByIdQuery(Guid Id) : IRequest<AssemblyDetailDto>;
