using Modules.Contracts.DTOs.AssemblyParts;
using Modules.Domain.Entities.Shared;

namespace Modules.Contracts.DTOs.Assemblies;

public sealed record AssemblyDto(
    Guid Id,
    string CategoryId,
    string Type,
    string Name,
    Dictionary<string, double> Parameters,
    Dictionary<string, ParamConstraint>? ParamConstraints,
    bool IsActive,
    DateTime CreatedAt,
    List<AssemblyPartDto> Parts);
