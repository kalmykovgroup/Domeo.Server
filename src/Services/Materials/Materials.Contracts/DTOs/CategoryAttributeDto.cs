namespace Materials.Contracts.DTOs;

public sealed record CategoryAttributeDto(
    string Id,
    string CategoryId,
    string Name,
    string Type,
    string? Unit,
    List<string>? EnumValues);
