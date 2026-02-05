using Domeo.Shared.Application;
using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Application.Commands.Projects;
using Projects.Domain.Entities;
using Projects.Domain.Repositories;

namespace Projects.API.Application.Projects.Commands;

public sealed class UpdateProjectStatusCommandHandler : IRequestHandler<UpdateProjectStatusCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectStatusCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateProjectStatusCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            throw new NotFoundException("Project", request.Id);

        if (!Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
            throw new DomeoValidationException("Status", "Invalid status");

        project.UpdateStatus(status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
