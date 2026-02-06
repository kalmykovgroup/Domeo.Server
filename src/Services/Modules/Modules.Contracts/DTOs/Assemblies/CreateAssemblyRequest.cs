using Modules.Domain.Entities.Shared;

namespace Modules.Contracts.DTOs.Assemblies;

public sealed record CreateAssemblyRequest(
    string CategoryId,
    string Type,
    string Name,
    Dictionary<string, double> Parameters,
    Dictionary<string, ParamConstraint>? ParamConstraints);
