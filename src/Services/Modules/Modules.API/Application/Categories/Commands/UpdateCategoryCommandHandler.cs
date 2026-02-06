using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Categories;
using Modules.Contracts.DTOs.Categories;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Categories.Commands;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ModuleCategoryDto>
{
    private readonly IModuleCategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(IModuleCategoryRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ModuleCategoryDto> Handle(
        UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category '{request.Id}' not found");

        category.Update(request.Name, request.Description, request.OrderIndex);

        _repository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ModuleCategoryDto(
            category.Id, category.ParentId, category.Name,
            category.Description, category.OrderIndex, category.IsActive);
    }
}
