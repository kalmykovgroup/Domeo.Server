using MediatR;

namespace Projects.Abstractions.Commands.Projects;

public sealed record DeleteProjectCommand(Guid Id) : IRequest;
