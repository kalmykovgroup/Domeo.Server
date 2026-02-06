using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Assemblies;
using Modules.Contracts.DTOs.Assemblies;
using Modules.Domain.Entities;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Assemblies.Commands;

public sealed class CreateAssemblyCommandHandler : IRequestHandler<CreateAssemblyCommand, AssemblyDto>
{
    private readonly IAssemblyRepository _assemblyRepository;
    private readonly IModuleCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAssemblyCommandHandler(
        IAssemblyRepository assemblyRepository,
        IModuleCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _assemblyRepository = assemblyRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssemblyDto> Handle(
        CreateAssemblyCommand request, CancellationToken cancellationToken)
    {
        _ = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Category '{request.CategoryId}' not found");

        var assembly = Assembly.Create(
            request.CategoryId,
            request.Type,
            request.Name,
            request.Parameters,
            request.ParamConstraints);

        _assemblyRepository.Add(assembly);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AssemblyDto(
            assembly.Id, assembly.CategoryId, assembly.Type, assembly.Name,
            assembly.Parameters, assembly.ParamConstraints,
            assembly.IsActive, assembly.CreatedAt, []);
    }
}
