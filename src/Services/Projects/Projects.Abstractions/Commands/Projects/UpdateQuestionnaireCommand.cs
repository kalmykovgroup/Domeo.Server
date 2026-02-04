using Domeo.Shared.Kernel.Application.Abstractions;

namespace Projects.Abstractions.Commands.Projects;

public sealed record UpdateQuestionnaireCommand(
    Guid Id,
    string? QuestionnaireData) : ICommand;
