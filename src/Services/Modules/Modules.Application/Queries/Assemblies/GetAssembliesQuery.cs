using MediatR;
using Modules.Contracts.DTOs.Assemblies;

namespace Modules.Application.Queries.Assemblies;

public sealed record GetAssembliesQuery(
    string? CategoryId,
    bool? ActiveOnly,
    string? Search,
    int? Page,
    int? Limit) : IRequest<AssembliesResponse>;
