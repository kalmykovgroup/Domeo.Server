using MediatR;

namespace Projects.Application.Commands.Projects;

public sealed record CreateProjectCommand(
    string Name,
    string Type,
    Guid ClientId,
    string? Notes) : IRequest<Guid>;
