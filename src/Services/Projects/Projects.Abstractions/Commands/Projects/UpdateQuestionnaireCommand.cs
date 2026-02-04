using MediatR;

namespace Projects.Abstractions.Commands.Projects;

public sealed record UpdateQuestionnaireCommand(
    Guid Id,
    string? QuestionnaireData) : IRequest;
