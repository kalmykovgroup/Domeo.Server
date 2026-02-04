using System.Text.Json;
using Domeo.Shared.Contracts.Events;
using Domeo.Shared.Infrastructure.Redis;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;

namespace Domeo.Shared.Infrastructure.Logging;

public sealed class RedisSink : ILogEventSink
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _serviceName;
    private IEventPublisher? _publisher;
    private bool _initialized;

    public RedisSink(IServiceProvider serviceProvider, string serviceName)
    {
        _serviceProvider = serviceProvider;
        _serviceName = serviceName;
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level < LogEventLevel.Error)
            return;

        try
        {
            // Lazy initialization to avoid circular dependency during startup
            if (!_initialized)
            {
                _publisher = _serviceProvider.GetService<IEventPublisher>();
                _initialized = true;
            }

            if (_publisher is null)
                return;

            var errorEvent = CreateErrorEvent(logEvent);
            _publisher.PublishErrorAsync(errorEvent).GetAwaiter().GetResult();
        }
        catch
        {
            // Sink should not throw exceptions
        }
    }

    private ApplicationErrorEvent CreateErrorEvent(LogEvent logEvent)
    {
        string? requestPath = null;
        Guid? userId = null;
        Dictionary<string, object>? properties = null;

        if (logEvent.Properties.Count > 0)
        {
            properties = new Dictionary<string, object>();

            foreach (var prop in logEvent.Properties)
            {
                if (prop.Key == "RequestPath" && prop.Value is ScalarValue requestPathValue)
                {
                    requestPath = requestPathValue.Value?.ToString();
                }
                else if (prop.Key == "UserId" && prop.Value is ScalarValue userIdValue)
                {
                    if (Guid.TryParse(userIdValue.Value?.ToString(), out var parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }
                else
                {
                    properties[prop.Key] = prop.Value.ToString();
                }
            }
        }

        var correlationId = logEvent.Properties.TryGetValue("CorrelationId", out var corrId)
            ? corrId.ToString().Trim('"')
            : Guid.NewGuid().ToString();

        return new ApplicationErrorEvent
        {
            ServiceName = _serviceName,
            Level = logEvent.Level.ToString(),
            Message = logEvent.RenderMessage(),
            Exception = logEvent.Exception?.ToString(),
            ExceptionType = logEvent.Exception?.GetType().FullName,
            Properties = properties?.Count > 0 ? JsonSerializer.Serialize(properties) : null,
            RequestPath = requestPath,
            UserId = userId,
            CorrelationId = correlationId
        };
    }
}
