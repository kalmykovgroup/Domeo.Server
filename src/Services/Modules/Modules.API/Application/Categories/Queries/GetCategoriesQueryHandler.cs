using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Modules.Abstractions.Queries.Categories;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.Categories.Queries;

public sealed class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, List<ModuleCategoryDto>>
{
    private readonly IModuleCategoryRepository _repository;

    public GetCategoriesQueryHandler(IModuleCategoryRepository repository)
        => _repository = repository;

    public async Task<Result<List<ModuleCategoryDto>>> Handle(
        GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _repository.GetCategoriesAsync(request.ActiveOnly, cancellationToken);

        var dtos = categories.Select(c => new ModuleCategoryDto(
            c.Id, c.ParentId, c.Name, c.Description, c.OrderIndex, c.IsActive)).ToList();

        return Result.Success(dtos);
    }
}
