using Domeo.Shared.Domain;

namespace Audit.Domain.Entities;

public sealed class LoginSession : Entity<Guid>
{
    public Guid UserId { get; private set; }
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
        string userRole,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new LoginSession
        {
            Id = sessionId,
            UserId = userId,
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
