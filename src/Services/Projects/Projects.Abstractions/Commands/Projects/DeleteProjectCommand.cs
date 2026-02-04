using Domeo.Shared.Kernel.Application.Abstractions;

namespace Projects.Abstractions.Commands.Projects;

public sealed record DeleteProjectCommand(Guid Id) : ICommand;
