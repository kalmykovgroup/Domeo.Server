namespace Domeo.Shared.Contracts.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string Name,
    string Role);
