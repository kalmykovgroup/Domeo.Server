using Domeo.Shared.Infrastructure.Resilience;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Domeo.Shared.Infrastructure.HealthChecks;

public sealed class DegradedRedisHealthCheck : IHealthCheck
{
    private readonly IConnectionStateTracker _stateTracker;

    public DegradedRedisHealthCheck(IConnectionStateTracker stateTracker)
    {
        _stateTracker = stateTracker;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_stateTracker.IsRedisAvailable)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Redis connection is available"));
        }

        return Task.FromResult(HealthCheckResult.Degraded("Redis connection is unavailable"));
    }
}
