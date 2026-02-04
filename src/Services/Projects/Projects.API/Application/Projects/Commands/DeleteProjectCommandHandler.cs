using Domeo.Shared.Application;
using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Abstractions.Commands.Projects;
using Projects.Abstractions.Repositories;

namespace Projects.API.Application.Projects.Commands;

public sealed class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            throw new NotFoundException("Project", request.Id);

        project.SoftDelete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
