using MediatR;
using Modules.Application.Queries.Categories;
using Modules.Contracts.DTOs.Categories;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Categories.Queries;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<ModuleCategoryDto>>
{
    private readonly IModuleCategoryRepository _repository;

    public GetCategoriesQueryHandler(IModuleCategoryRepository repository)
        => _repository = repository;

    public async Task<List<ModuleCategoryDto>> Handle(
        GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _repository.GetCategoriesAsync(request.ActiveOnly, cancellationToken);

        return categories.Select(c => new ModuleCategoryDto(
            c.Id, c.ParentId, c.Name, c.Description, c.OrderIndex, c.IsActive)).ToList();
    }
}
