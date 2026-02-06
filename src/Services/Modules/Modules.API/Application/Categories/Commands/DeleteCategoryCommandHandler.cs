using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Categories;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Categories.Commands;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IModuleCategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(IModuleCategoryRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category '{request.Id}' not found");

        _repository.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
