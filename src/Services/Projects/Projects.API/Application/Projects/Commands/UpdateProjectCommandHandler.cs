using Domeo.Shared.Application;
using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Abstractions.Commands.Projects;
using Projects.Abstractions.Repositories;

namespace Projects.API.Application.Projects.Commands;

public sealed class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            throw new NotFoundException("Project", request.Id);

        project.Update(request.Name, request.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
