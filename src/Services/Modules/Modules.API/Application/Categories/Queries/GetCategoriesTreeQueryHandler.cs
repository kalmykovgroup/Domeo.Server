using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Modules.Abstractions.Queries.Categories;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.Categories.Queries;

public sealed class GetCategoriesTreeQueryHandler : IQueryHandler<GetCategoriesTreeQuery, List<ModuleCategoryTreeDto>>
{
    private readonly IModuleCategoryRepository _repository;

    public GetCategoriesTreeQueryHandler(IModuleCategoryRepository repository)
        => _repository = repository;

    public async Task<Result<List<ModuleCategoryTreeDto>>> Handle(
        GetCategoriesTreeQuery request, CancellationToken cancellationToken)
    {
        var allCategories = await _repository.GetCategoriesAsync(request.ActiveOnly, cancellationToken);

        var categoryDict = allCategories.ToDictionary(
            c => c.Id,
            c => new ModuleCategoryTreeDto(c.Id, c.ParentId, c.Name, c.Description, c.OrderIndex, c.IsActive));

        var rootCategories = new List<ModuleCategoryTreeDto>();

        foreach (var category in allCategories)
        {
            var treeNode = categoryDict[category.Id];

            if (category.ParentId is not null && categoryDict.TryGetValue(category.ParentId, out var parent))
            {
                parent.Children.Add(treeNode);
            }
            else
            {
                rootCategories.Add(treeNode);
            }
        }

        return Result.Success(rootCategories);
    }
}
