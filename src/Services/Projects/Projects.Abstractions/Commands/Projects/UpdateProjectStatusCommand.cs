using Domeo.Shared.Kernel.Application.Abstractions;

namespace Projects.Abstractions.Commands.Projects;

public sealed record UpdateProjectStatusCommand(
    Guid Id,
    string Status) : ICommand;
