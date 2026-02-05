using MediatR;

namespace Modules.Application.Queries.Assemblies;

public sealed record GetAssembliesCountQuery(
    string? CategoryId,
    bool? ActiveOnly,
    string? Search) : IRequest<int>;
