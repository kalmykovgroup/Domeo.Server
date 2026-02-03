using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Domeo.Shared.Auth.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Domeo.Shared.Auth;

public static class DependencyInjection
{
    // Header names from API Gateway
    public const string UserIdHeader = "X-User-Id";
    public const string UserEmailHeader = "X-User-Email";
    public const string UserNameHeader = "X-User-Name";
    public const string UserRoleHeader = "X-User-Role";

    /// <summary>
    /// Add authentication for microservices that trust API Gateway headers
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

        // Register permission-based authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, InternalApiKeyHandler>();

        return services;
    }

    /// <summary>
    /// Legacy: Add JWT authentication with symmetric key (for services that validate tokens directly)
    /// </summary>
    public static IServiceCollection AddSharedAuthWithJwt(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<ICurrentUserAccessor, CurrentUserAccessor>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };

            // Extract token from cookie if not in header
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue("access_token", out var token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddHttpContextAccessor();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("InternalApi", policy =>
                policy.Requirements.Add(new InternalApiKeyRequirement()));
        });

        // Register permission-based authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
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
        var name = Request.Headers[DependencyInjection.UserNameHeader].FirstOrDefault() ?? "";
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
