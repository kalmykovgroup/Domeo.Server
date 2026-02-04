using Domeo.Shared.Infrastructure.Resilience;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Domeo.Shared.Infrastructure.HealthChecks;

public sealed class DegradedDatabaseHealthCheck : IHealthCheck
{
    private readonly IConnectionStateTracker _stateTracker;

    public DegradedDatabaseHealthCheck(IConnectionStateTracker stateTracker)
    {
        _stateTracker = stateTracker;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_stateTracker.IsDatabaseAvailable)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Database connection is available"));
        }

        return Task.FromResult(HealthCheckResult.Degraded("Database connection is unavailable"));
    }
}
