namespace Audit.Abstractions.DTOs;

public sealed record LoginSessionDto(
    Guid Id,
    Guid UserId,
    string UserRole,
    string? IpAddress,
    string? UserAgent,
    DateTime LoggedInAt,
    DateTime? LoggedOutAt,
    bool IsActive);
