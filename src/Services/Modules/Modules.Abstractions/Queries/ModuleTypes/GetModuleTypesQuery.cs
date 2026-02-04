using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Modules.Abstractions.Queries.ModuleTypes;

public sealed record GetModuleTypesQuery(
    string? CategoryId,
    bool? ActiveOnly,
    string? Search,
    int? Page,
    int? Limit) : IQuery<ModuleTypesResponse>;

public sealed record ModuleTypesResponse(
    List<ModuleTypeDto> Items,
    int? Total = null,
    int? Page = null,
    int? Limit = null)
{
    public bool IsPaginated => Total.HasValue;
}
