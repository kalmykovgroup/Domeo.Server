using MediatR;
using Modules.Contracts.DTOs.Assemblies;

namespace Modules.Application.Queries.Assemblies;

public sealed record GetAssemblyByIdQuery(Guid Id) : IRequest<AssemblyDetailDto>;
