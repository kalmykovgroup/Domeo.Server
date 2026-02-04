using Domeo.Shared.Kernel.Application.Abstractions;

namespace Projects.Abstractions.Commands.Projects;

public sealed record UpdateProjectCommand(
    Guid Id,
    string Name,
    string? Notes) : ICommand;
