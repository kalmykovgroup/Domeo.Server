namespace Clients.Abstractions.DTOs;

public sealed record ClientDto(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes,
    Guid UserId,
    DateTime CreatedAt,
    DateTime? DeletedAt);
