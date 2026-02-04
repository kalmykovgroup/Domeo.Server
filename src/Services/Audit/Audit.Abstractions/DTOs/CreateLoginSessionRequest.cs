namespace Audit.Abstractions.DTOs;

public sealed record CreateLoginSessionRequest(
    Guid UserId,
    string UserRole,
    string? IpAddress,
    string? UserAgent);
