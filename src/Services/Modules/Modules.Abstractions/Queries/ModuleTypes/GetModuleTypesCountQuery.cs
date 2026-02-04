using Domeo.Shared.Kernel.Application.Abstractions;

namespace Modules.Abstractions.Queries.ModuleTypes;

public sealed record GetModuleTypesCountQuery(
    string? CategoryId,
    bool? ActiveOnly,
    string? Search) : IQuery<int>;
