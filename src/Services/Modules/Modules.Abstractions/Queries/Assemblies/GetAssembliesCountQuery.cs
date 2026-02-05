using MediatR;

namespace Modules.Abstractions.Queries.Assemblies;

public sealed record GetAssembliesCountQuery(
    string? CategoryId,
    bool? ActiveOnly,
    string? Search) : IRequest<int>;
