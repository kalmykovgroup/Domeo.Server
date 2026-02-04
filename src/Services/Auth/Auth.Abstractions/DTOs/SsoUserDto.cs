namespace Auth.Abstractions.DTOs;

public sealed record SsoUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role);
