using Modules.Domain.Entities.Shared;

namespace Modules.Contracts.DTOs.Components;

public sealed record ComponentDto(
    Guid Id,
    string Name,
    List<string> Tags,
    ComponentParams? Params,
    string? Color,
    bool IsActive,
    DateTime CreatedAt);
