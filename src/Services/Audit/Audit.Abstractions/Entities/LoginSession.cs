using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Audit.Abstractions.Entities;

public sealed class LoginSession : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string UserEmail { get; private set; } = string.Empty;
    public string? UserName { get; private set; }
    public string UserRole { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime LoggedInAt { get; private set; }
    public DateTime? LoggedOutAt { get; private set; }
    public bool IsActive => !LoggedOutAt.HasValue;

    private LoginSession() { }

    public static LoginSession Create(
        Guid sessionId,
        Guid userId,
        string userEmail,
        string? userName,
        string userRole,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new LoginSession
        {
            Id = sessionId,
            UserId = userId,
            UserEmail = userEmail,
            UserName = userName,
            UserRole = userRole,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            LoggedInAt = DateTime.UtcNow
        };
    }

    public void Logout()
    {
        LoggedOutAt = DateTime.UtcNow;
    }
}
