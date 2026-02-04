using MediatR;

namespace Projects.Abstractions.Commands.Projects;

public sealed record CreateProjectCommand(
    string Name,
    string Type,
    Guid ClientId,
    string? Notes) : IRequest<Guid>;
