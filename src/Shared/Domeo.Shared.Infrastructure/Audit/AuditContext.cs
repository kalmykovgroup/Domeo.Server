namespace Domeo.Shared.Infrastructure.Audit;

public sealed class AuditContext
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
