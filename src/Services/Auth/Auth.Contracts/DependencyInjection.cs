using System.Security.Claims;
using System.Text.Encodings.Web;
using Auth.Contracts.Authorization;
using Auth.Contracts.Routes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auth.Contracts;

public static class DependencyInjection
{
    /// <summary>
    /// Add authentication for microservices that trust API Gateway headers.
    /// Uses role-based authorization via [Authorize(Roles = "...")].
    /// </summary>
    public static IServiceCollection AddAuthContracts(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Use header-based authentication (trust API Gateway)
        services.AddAuthentication("GatewayHeaders")
            .AddScheme<AuthenticationSchemeOptions, GatewayHeadersAuthenticationHandler>(
                "GatewayHeaders", null);

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder("GatewayHeaders")
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy("InternalApi", policy =>
                policy.Requirements.Add(new InternalApiKeyRequirement()));
        });

        // Register internal API key handler
        services.AddSingleton<IAuthorizationHandler, InternalApiKeyHandler>();

        return services;
    }
}

/// <summary>
/// Authentication handler that reads user claims from API Gateway headers
/// </summary>
public class GatewayHeadersAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public GatewayHeadersAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(AuthRoutes.Headers.UserId, out var userIdValues) ||
            string.IsNullOrEmpty(userIdValues.FirstOrDefault()))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = userIdValues.First()!;
        var role = Request.Headers[AuthRoutes.Headers.UserRole].FirstOrDefault() ?? "";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("sub", userId)
        };

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
