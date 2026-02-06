using Modules.Domain.Entities.Shared;

namespace Modules.Contracts.DTOs.Assemblies;

public sealed record UpdateAssemblyRequest(
    string Name,
    Dictionary<string, double> Parameters,
    Dictionary<string, ParamConstraint>? ParamConstraints);
