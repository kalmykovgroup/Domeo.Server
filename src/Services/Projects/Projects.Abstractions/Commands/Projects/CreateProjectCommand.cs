using Domeo.Shared.Kernel.Application.Abstractions;

namespace Projects.Abstractions.Commands.Projects;

public sealed record CreateProjectCommand(
    string Name,
    string Type,
    Guid ClientId,
    string? Notes) : ICommand<Guid>;
