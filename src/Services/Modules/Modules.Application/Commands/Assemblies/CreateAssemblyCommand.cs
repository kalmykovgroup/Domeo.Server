using MediatR;
using Modules.Contracts.DTOs.Assemblies;
using Modules.Domain.Entities.Shared;

namespace Modules.Application.Commands.Assemblies;

public sealed record CreateAssemblyCommand(
    string CategoryId,
    string Type,
    string Name,
    Dictionary<string, double> Parameters,
    Dictionary<string, ParamConstraint>? ParamConstraints) : IRequest<AssemblyDto>;
