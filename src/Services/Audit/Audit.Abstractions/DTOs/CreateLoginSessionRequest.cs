namespace Audit.Abstractions.DTOs;

public sealed record CreateLoginSessionRequest(
    Guid UserId,
    string UserEmail,
    string? UserName,
    string UserRole,
    string? IpAddress,
    string? UserAgent);
