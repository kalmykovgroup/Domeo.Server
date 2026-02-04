using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Auth.Contracts.Authorization;

public sealed class InternalApiKeyHandler : AuthorizationHandler<InternalApiKeyRequirement>
{
    private const string ApiKeyHeader = "X-Internal-Api-Key";
    private readonly string? _expectedApiKey;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InternalApiKeyHandler(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _expectedApiKey = configuration["InternalApi:Key"];
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InternalApiKeyRequirement requirement)
    {
        if (string.IsNullOrEmpty(_expectedApiKey))
        {
            // If no key configured, deny all internal requests
            return Task.CompletedTask;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return Task.CompletedTask;
        }

        var providedApiKey = httpContext.Request.Headers[ApiKeyHeader].FirstOrDefault();

        if (!string.IsNullOrEmpty(providedApiKey) && providedApiKey == _expectedApiKey)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
