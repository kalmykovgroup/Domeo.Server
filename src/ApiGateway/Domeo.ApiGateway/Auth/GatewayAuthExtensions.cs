using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Domeo.ApiGateway.Auth;

public static class GatewayAuthExtensions
{
    public static IServiceCollection AddGatewayAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authority = configuration["AuthCenter:Authority"] ?? "http://localhost:5100";
        var audience = configuration["AuthCenter:Audience"] ?? "domeo";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // JWKS endpoint for public key
            options.Authority = authority;
            options.MetadataAddress = $"{authority}/.well-known/openid-configuration";
            options.RequireHttpsMetadata = false; // Dev only

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authority,
                ValidAudience = audience,
                ClockSkew = TimeSpan.Zero
            };

            // Extract token from cookie if not in header
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Try to get token from cookie
                    if (context.Request.Cookies.TryGetValue("access_token", out var token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
