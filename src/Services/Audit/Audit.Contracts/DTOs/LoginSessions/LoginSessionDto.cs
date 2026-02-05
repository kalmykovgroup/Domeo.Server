namespace Audit.Contracts.DTOs.LoginSessions;

public sealed record LoginSessionDto(
    Guid Id,
    Guid UserId,
    string UserRole,
    string? IpAddress,
    string? UserAgent,
    DateTime LoggedInAt,
    DateTime? LoggedOutAt,
    bool IsActive);
