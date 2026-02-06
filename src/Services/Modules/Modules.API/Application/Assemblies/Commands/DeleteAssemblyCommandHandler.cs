using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.Assemblies;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Assemblies.Commands;

public sealed class DeleteAssemblyCommandHandler : IRequestHandler<DeleteAssemblyCommand>
{
    private readonly IAssemblyRepository _assemblyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAssemblyCommandHandler(
        IAssemblyRepository assemblyRepository,
        IUnitOfWork unitOfWork)
    {
        _assemblyRepository = assemblyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteAssemblyCommand request, CancellationToken cancellationToken)
    {
        var assembly = await _assemblyRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Assembly {request.Id} not found");

        _assemblyRepository.Remove(assembly);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
