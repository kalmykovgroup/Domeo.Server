using Domeo.Shared.Application;
using MediatR;
using Modules.Application.Commands.AssemblyParts;
using Modules.Domain.Repositories;

namespace Modules.API.Application.AssemblyParts.Commands;

public sealed class DeleteAssemblyPartCommandHandler : IRequestHandler<DeleteAssemblyPartCommand>
{
    private readonly IAssemblyPartRepository _partRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAssemblyPartCommandHandler(IAssemblyPartRepository partRepository, IUnitOfWork unitOfWork)
    {
        _partRepository = partRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeleteAssemblyPartCommand request, CancellationToken cancellationToken)
    {
        var part = await _partRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"AssemblyPart {request.Id} not found");

        _partRepository.Remove(part);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
