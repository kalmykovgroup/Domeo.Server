namespace Modules.Contracts.DTOs.Storage;

public sealed record StorageConnectionDto(
    Guid Id,
    string Name,
    string Type,
    string Endpoint,
    string Bucket,
    string Region,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
