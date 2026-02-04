namespace Domeo.Shared.Infrastructure.Audit;

public interface IAuditContextAccessor
{
    AuditContext? AuditContext { get; set; }
}
