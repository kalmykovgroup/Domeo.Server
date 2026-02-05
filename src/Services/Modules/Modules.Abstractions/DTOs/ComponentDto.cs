using Modules.Abstractions.Entities.Shared;

namespace Modules.Abstractions.DTOs;

public sealed record ComponentDto(
    Guid Id,
    string Name,
    List<string> Tags,
    ComponentParams? Params,
    bool IsActive,
    DateTime CreatedAt);
