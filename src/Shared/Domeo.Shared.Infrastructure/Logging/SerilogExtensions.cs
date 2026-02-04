using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Domeo.Shared.Infrastructure.Logging;

public static class SerilogExtensions
{
    public static LoggerConfiguration Redis(
        this LoggerSinkConfiguration sinkConfiguration,
        IServiceProvider serviceProvider,
        string serviceName,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Error)
    {
        return sinkConfiguration.Sink(
            new RedisSink(serviceProvider, serviceName),
            restrictedToMinimumLevel);
    }
}
