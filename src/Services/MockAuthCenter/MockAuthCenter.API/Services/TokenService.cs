using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domeo.Shared.Auth;
using Microsoft.IdentityModel.Tokens;

namespace MockAuthCenter.API.Services;

public class TokenService
{
    private readonly RsaKeyService _rsaKeyService;
    private readonly UserStore _userStore;
    private readonly string _issuer;
    private readonly int _accessTokenLifetimeMinutes;

    public TokenService(
        RsaKeyService rsaKeyService,
        UserStore userStore,
        IConfiguration configuration)
    {
        _rsaKeyService = rsaKeyService;
        _userStore = userStore;
        _issuer = configuration["AuthCenter:Issuer"] ?? "http://localhost:5100";
        _accessTokenLifetimeMinutes = configuration.GetValue<int>("AuthCenter:AccessTokenLifetimeMinutes", 15);
    }

    public string GenerateAccessToken(TestUser user, string clientId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("client_id", clientId)
        };

        // Add role for the specific program/client
        var role = _userStore.GetRoleForProgram(user, clientId);
        if (role != null)
        {
            claims.Add(new Claim("role", role));

            // Add all role permissions to JWT for authorization
            var permissions = RolePermissions.GetPermissions(role);
            if (permissions.Count > 0)
            {
                claims.Add(new Claim("permissions", string.Join(",", permissions)));
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenLifetimeMinutes),
            Issuer = _issuer,
            Audience = clientId,
            SigningCredentials = _rsaKeyService.SigningCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public TokenResponse CreateTokenResponse(TestUser user, string clientId, string refreshToken)
    {
        var accessToken = GenerateAccessToken(user, clientId);

        return new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = _accessTokenLifetimeMinutes * 60,
            RefreshToken = refreshToken
        };
    }
}

public class TokenResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}
