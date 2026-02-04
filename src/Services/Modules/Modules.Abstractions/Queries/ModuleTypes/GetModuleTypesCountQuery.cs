using MediatR;

namespace Modules.Abstractions.Queries.ModuleTypes;

public sealed record GetModuleTypesCountQuery(
    string? CategoryId,
    bool? ActiveOnly,
    string? Search) : IRequest<int>;
