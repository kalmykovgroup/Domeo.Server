using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Categories;
using Modules.Contracts.DTOs.Categories;
using Modules.Domain.Entities;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Categories.Commands;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ModuleCategoryDto>
{
    private readonly IModuleCategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(IModuleCategoryRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ModuleCategoryDto> Handle(
        CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentId is not null)
        {
            _ = await _repository.GetByIdAsync(request.ParentId, cancellationToken)
                ?? throw new KeyNotFoundException($"Parent category '{request.ParentId}' not found");
        }

        var category = ModuleCategory.Create(
            request.Id, request.Name, request.ParentId, request.Description, request.OrderIndex);

        _repository.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ModuleCategoryDto(
            category.Id, category.ParentId, category.Name,
            category.Description, category.OrderIndex, category.IsActive);
    }
}
