namespace Audit.Contracts.DTOs.LoginSessions;

public sealed record CreateLoginSessionRequest(
    Guid UserId,
    string UserRole,
    string? IpAddress,
    string? UserAgent);
