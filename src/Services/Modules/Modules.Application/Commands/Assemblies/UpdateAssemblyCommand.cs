using MediatR;
using Modules.Contracts.DTOs.Assemblies;
using Modules.Domain.Entities.Shared;

namespace Modules.Application.Commands.Assemblies;

public sealed record UpdateAssemblyCommand(
    Guid Id,
    string Name,
    Dictionary<string, double> Parameters,
    Dictionary<string, ParamConstraint>? ParamConstraints) : IRequest<AssemblyDto>;
