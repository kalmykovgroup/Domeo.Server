using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Audit.API.Entities;

public sealed class ApplicationLog : Entity<Guid>
{
    public string ServiceName { get; private set; } = string.Empty;
    public string Level { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? Exception { get; private set; }
    public string? ExceptionType { get; private set; }
    public string? Properties { get; private set; }
    public string? RequestPath { get; private set; }
    public Guid? UserId { get; private set; }
    public string? CorrelationId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ApplicationLog() { }

    public static ApplicationLog Create(
        string serviceName,
        string level,
        string message,
        string? exception = null,
        string? exceptionType = null,
        string? properties = null,
        string? requestPath = null,
        Guid? userId = null,
        string? correlationId = null)
    {
        return new ApplicationLog
        {
            Id = Guid.NewGuid(),
            ServiceName = serviceName,
            Level = level,
            Message = message,
            Exception = exception,
            ExceptionType = exceptionType,
            Properties = properties,
            RequestPath = requestPath,
            UserId = userId,
            CorrelationId = correlationId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
