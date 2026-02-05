using MediatR;

namespace Projects.Application.Commands.Projects;

public sealed record UpdateQuestionnaireCommand(
    Guid Id,
    string? QuestionnaireData) : IRequest;
