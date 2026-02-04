namespace Domeo.Shared.Infrastructure.Audit;

public sealed class AuditContextAccessor : IAuditContextAccessor
{
    private static readonly AsyncLocal<AuditContext?> _context = new();

    public AuditContext? AuditContext
    {
        get => _context.Value;
        set => _context.Value = value;
    }
}
