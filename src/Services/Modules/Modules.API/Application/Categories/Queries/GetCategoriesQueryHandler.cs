using MediatR;
using Modules.Application.Queries.Categories;
using Modules.Contracts.DTOs.Categories;
using Modules.Domain.Entities;
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
        // EF Core relationship fixup auto-populates Children
        var allCategories = await _repository.GetCategoriesAsync(request.ActiveOnly, cancellationToken);

        return allCategories
            .Where(c => c.ParentId is null)
            .Select(MapToDto)
            .ToList();
    }

    private static ModuleCategoryDto MapToDto(ModuleCategory c) => new(
        c.Id, c.ParentId, c.Name, c.Description, c.OrderIndex, c.IsActive,
        c.Children.OrderBy(ch => ch.OrderIndex).Select(MapToDto).ToList());
}
