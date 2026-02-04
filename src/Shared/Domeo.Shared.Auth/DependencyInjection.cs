using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Web;
using Domeo.Shared.Auth.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Domeo.Shared.Auth;

public static class DependencyInjection
{
    // Header names from API Gateway
    public const string UserIdHeader = "X-User-Id";
    public const string UserEmailHeader = "X-User-Email";
    public const string UserNameHeader = "X-User-Name";
    public const string UserRoleHeader = "X-User-Role";

    /// <summary>
    /// Add authentication for microservices that trust API Gateway headers.
    /// Uses role-based authorization via [Authorize(Roles = "...")].
    /// </summary>
    public static IServiceCollection AddSharedAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<ICurrentUserAccessor, CurrentUserAccessor>();
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

    public static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app)
    {
        app.UseMiddleware<CurrentUserMiddleware>();
        return app;
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
        // Check for user ID header from API Gateway
        if (!Request.Headers.TryGetValue(DependencyInjection.UserIdHeader, out var userIdValues) ||
            string.IsNullOrEmpty(userIdValues.FirstOrDefault()))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = userIdValues.First()!;
        var email = Request.Headers[DependencyInjection.UserEmailHeader].FirstOrDefault() ?? "";
        // URL-decode name (may contain non-ASCII characters like Cyrillic)
        var nameRaw = Request.Headers[DependencyInjection.UserNameHeader].FirstOrDefault() ?? "";
        var name = HttpUtility.UrlDecode(nameRaw);
        var role = Request.Headers[DependencyInjection.UserRoleHeader].FirstOrDefault() ?? "";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("sub", userId)
        };

        if (!string.IsNullOrEmpty(email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim("email", email));
        }

        if (!string.IsNullOrEmpty(name))
        {
            claims.Add(new Claim(ClaimTypes.Name, name));
            claims.Add(new Claim("name", name));
        }

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
