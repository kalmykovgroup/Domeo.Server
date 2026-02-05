using Domeo.Shared.Application;
using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Application.Commands.Projects;
using Projects.Domain.Repositories;

namespace Projects.API.Application.Projects.Commands;

public sealed class UpdateQuestionnaireCommandHandler : IRequestHandler<UpdateQuestionnaireCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuestionnaireCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateQuestionnaireCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            throw new NotFoundException("Project", request.Id);

        project.SetQuestionnaireData(request.QuestionnaireData);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
