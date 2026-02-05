using MediatR;
using Modules.Abstractions.DTOs;

namespace Modules.Abstractions.Queries.Assemblies;

public sealed record GetAssembliesQuery(
    string? CategoryId,
    bool? ActiveOnly,
    string? Search,
    int? Page,
    int? Limit) : IRequest<AssembliesResponse>;

public sealed record AssembliesResponse(
    List<AssemblyDto> Items,
    int? Total = null,
    int? Page = null,
    int? Limit = null)
{
    public bool IsPaginated => Total.HasValue;
}
